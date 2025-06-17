import os
import numpy as np
from mask2bbox import BBoxes
import tifffile as tiff
import sys
import pandas as pd
import json
import pandas as pd
import numpy as np
import os
from PIL import Image
import ast
import argparse
from helperfunctions import save2DFcolumn
from skimage.transform import resize


sys.path.append(os.path.dirname(os.path.dirname(
    os.path.dirname(os.path.abspath(__file__)))))


def main(save_dir: str,
         mask_path: str,
         img_path: str,
         n: int,
         target_a_ratio: float, downsample: tuple[float], **kwargs: dict
         ):
    """
    Processes an image and its corresponding mask to extract and save single-cell patches.
    This function performs the following steps:
    1. Creates the save directory if it does not exist.
    2. Generates bounding boxes from the mask and image.
    3. Expands the bounding boxes by a specified number of pixels.
    4. Removes bounding boxes that are located on the edge of the image.
    5. Filters bounding boxes based on the maximum allowed side length.
    6. Reads the image and assigns it to the filtered bounding boxes.
    7. Saves the filtered bounding box coordinates to a text file.
    8. Calculates resizing factors to match the target size and aspect ratio.
    9. Creates a directory for saving image patches if it does not exist.
    10. Extracts and saves resized image patches for each bounding box.
    Args:
        save_dir (str): Directory where results and patches will be saved.
        mask_path (str): Path to the mask image file.
        img_path (str): Path to the original image file.
        n (int): Number of pixels to expand each bounding box.
        max_side (int): Maximum allowed side length for bounding boxes.
        target_size (int): Target size (width and height) for the output patches.
        target_a_ratio (float): Desired aspect ratio for the output patches.
    Returns:
        None
    """

    csv_path = os.path.join(save_dir, "bbox.csv")

    if os.path.exists(csv_path) and os.path.exists(os.path.join(save_dir, "mask_downsampled.png")):
        df = pd.read_csv(csv_path)
        return df
    else:
        df = generate_data(csv_path, save_dir,
                           mask_path, img_path, n, target_a_ratio, downsample, **kwargs)

    return df


def generate_data(csv_path: str, save_dir: str,
                  mask_path: str,
                  img_path: str,
                  n: int,
                  target_a_ratio: float, downsample: tuple[float], **kwargs: dict
                  ) -> pd.DataFrame:
    # Downsample if instructed
    if downsample is not None:
        # Read in the mask file
        n = 1
        mask = load_img(mask_path)

        mask_path = os.path.join(save_dir, "mask_downsampled.tif")

        if not os.path.exists(mask_path) and not os.path.exists(img_path):
            mask = resize(mask, downsample,
                          order=0).astype(np.uint16)
            save_img(mask, mask_path, 'tiff')
            logger.info(
                f"Mask saved to {mask_path} and mask size is {mask.shape}")

        else:
            mask = load_img(mask_path)

    # Extract bounding boxes from the mask
    all_boxes: BBoxes = BBoxes.from_mask(mask_path)

    assert all_boxes.bboxes.shape[0] > 0, "No bounding boxes found in the mask."

    all_boxes: BBoxes = all_boxes.expand(n=n)

    all_boxes: BBoxes = all_boxes.remove_from_edge()

    assert np.all(all_boxes.bboxes >
                  0), "No bounding boxes found after expansion and edge removal."

    # Filter bounding boxes
    if downsample is None:
        # min max scaling to uint16
        img = load_img(img_path)
        img_path = os.path.join(save_dir, "img.png")
        img = (img - img.min()) / (img.max() - img.min()) * 255**2
        save_img(img.astype(np.uint16), img_path, 'PNG')
        all_boxes, sides = get_filtered_boxes(all_boxes, save_dir, csv_path)

    df = pd.DataFrame(all_boxes.bboxes, columns=[
        "label_ids", "X1", "X2", "Y1", "Y2"])

    # see if indi

    # Unify the dataframes between downsampled and non-downsampled bounding boxes
    if downsample is not None:
        if os.path.exists(csv_path):
            df_existing = pd.read_csv(csv_path)

            # TODO: Log to confirm if indices are the same as we get Nan error for bbx when merfing dfs

            down_bbox_cols = [
                f"{x}_downsampled" for x in ["X1", "X2", "Y1", "Y2"]]
            if df_existing.columns.isin(down_bbox_cols).all():
                df_existing = df_existing.drop(columns=down_bbox_cols)

            df = df_existing.merge(
                df, on="label_ids", how="left", suffixes=("", "_downsampled"))

            # Test
            all_boxes.image = img
            del mask, img
            mask = load_img(mask_path.replace("_downsampled", ""))
            all_boxes.mask = mask
            all_boxes.bboxes = df_existing[[
                "label_ids", "X1", "X2", "Y1", "Y2"]].values
            img, canvas = all_boxes.draw(idx=0, to="image", method="numpy")
            img = img + canvas[..., 0]
            img = (img - img.min()) / (img.max() - img.min()) * 255
            save_img(img.astype(np.uint8), os.path.join(save_dir, "img.png"))

    for col in df.columns:
        if np.issubdtype(df[col].dtypes, np.number):
            df[col] = df[col].fillna(0)
            df[col] = df[col].astype(int)

    # Crop the patches
    if downsample is None:
        patch_dir = performcropping(all_boxes, sides,
                                    target_a_ratio, img, save_dir)

        df["Image_path"] = df["label_ids"].apply(
            lambda x: os.path.join(os.path.dirname(patch_dir), f"img_{x}.png"))

        df["whole_slide_img_ndim"] = all_boxes.image.ndim

        logger.info(
            f"Dims: {all_boxes.image.ndim} and shape: {all_boxes.image.shape}")

        dim_dict = {0: "Y", 1: "X", 3: "C", 2: "Z", 4: "T"}

        for dim in range(all_boxes.image.ndim):
            value = dim_dict[dim]
            df[f"whole_slide_img_shape_{value}"] = all_boxes.image.shape[dim]

    # Save to CSV
    df.to_csv(csv_path, index=False)

    return df


def get_filtered_boxes(all_boxes: BBoxes, save_dir: str,
                       csv_path: str) -> [BBoxes, np.ndarray]:

    if not os.path.exists(save_dir) or not [s for s in os.listdir(save_dir) if s.endswith(".png")] \
            or not os.path.exists(csv_path):

        os.makedirs(save_dir, exist_ok=True)

        logger.info(f"Directory created: {save_dir}")

        sides: np.ndarray = all_boxes.get_sides()[1:, ...]

        third_quartile_cols: int = int(np.quantile(sides[:, 0], 0.95))
        third_quartile_rows: int = int(np.quantile(sides[:, 1], 0.95))

        logger.info(
            f"Filtering bounding boxes with sides <= ({third_quartile_cols}, {third_quartile_rows}).")
        all_boxes: BBoxes = all_boxes.filter("sides", np.less_equal,
                                             (third_quartile_cols, third_quartile_rows))

    return all_boxes, sides


def performcropping(all_boxes: BBoxes, sides: np.ndarray,
                    target_a_ratio: float, img: np.ndarray, save_dir: str) -> str:

    target_size: int = int(np.median(sides.flatten()))

    logger.info(f"Calculating resize factors for target size {target_size} and \
                aspect ratio {target_a_ratio}.")

    resize_factors: np.ndarray = all_boxes.calculate_resizing_factor(
        desired_ratio=target_a_ratio, size=(target_size, target_size))

    patch_dir: str = os.path.join(save_dir, "patches")
    if not os.path.exists(patch_dir):
        os.makedirs(patch_dir)
        logger.info(f"Patch directory created: {patch_dir}")
    else:
        logger.info(f"Patch directory already exists: {patch_dir}")

    patch_dir: str = os.path.join(patch_dir, "img")

    all_boxes.image = img

    logger.info(f"Extracting and saving patches to {patch_dir}.")

    all_boxes.extract(resize_factors, size=(target_size,
                                            target_size),
                      output=patch_dir, rescale_intensity=True)

    logger.info("Extraction and saving of patches completed.")

    return patch_dir


def save_img(img, file_path: str, format: str = 'PNG'):

    if "tif" in os.path.splitext(file_path)[-1]:
        tiff.imwrite(file_path, img)
    else:
        Image.fromarray(img).save(file_path, format=format)


def drawbbox(filtered_boxes: BBoxes) -> None:
    # Save array as png
    img, canvas = filtered_boxes.draw(idx=0, to="image", method="numpy")
    img = img[..., None] + canvas
    return img


def get_bbox_from_csv(data_dir) -> pd.DataFrame:
    file_path = os.path.join(data_dir, "bbox.txt")

    if not os.path.exists(file_path):
        raise FileNotFoundError(f"bbox not found in {file_path}")

    df = pd.read_csv(file_path, sep=",", names=[
                     "label_ids", "X1", "X2", "Y1", "Y2", "whole_slide_img_shape"])
    # Set 'N' as the index if it exists
    df = df.set_index("N")

    # Replace 'inf' and 'NaN' values with 0
    sanitize_dataframe(df)

    # Convert all columns to integer in a vectorized way
    df = df.astype(int)

    return df


def load_img(file_path) -> np.ndarray:

    if not os.path.exists(file_path):
        raise FileNotFoundError(f"img not found in {file_path}")
    img = Image.open(file_path)
    img = np.array(img)

    return img


def img_encode4json(data_dir, img) -> dict:

    shape = img.shape
    img = img.flatten().tolist()

    df = {"shape": shape, "img": img}
    return df


def sanitize_dataframe(df: pd.DataFrame) -> pd.DataFrame:

    if [np.inf, -np.inf] in df.values:
        logger.warning("DataFrame contains infinite values. Replacing with 0.")
        df.replace([np.inf, -np.inf], 0, inplace=True)  # In memory replacement

    if df.isnull().values.any():
        logger.warning("DataFrame contains NaN values. Replacing with 0.")
        # Replace NaN values with 0
        df.fillna(0, inplace=True)  # In memory replacement


def json_serialize(df, data_dir):

    if os.path.exists(data_dir):

        # Convert DataFrame to JSON in the required format

        """When a pandas DataFrame containing object dtype columns is serialized using 
        .to_json(), pandas serializes based on the actual runtime value of each object, not the dtype"""
        # img_df = load_img(data_dir)

        # Works with the following C# format:

        # public class DataFrame
        # {   public List<int> N;
        #    public List<int> X1;
        #    public List<int> X2;
        #    public List<int> Y1;
        #    public List<int> Y2;
        #    public List<int[]> img;
        # }

        """Each list in a DataFrame cell is preserved as a nested list in the JSON.

Lists are converted recursively and safely to JSON arrays.

Strings, ints, floats, dicts, and nested lists all serialize cleanly."""
        bbox = df.to_dict(orient="list")
        bbox = json.dumps(bbox)

        return bbox
    else:
        raise FileNotFoundError(f"Directory {data_dir} does not exist")


def get_args(arg_parser):
    # Add an argument to the parser
    arg_parser.add_argument("--mask_path", type=str,
                            help="Path to the mask image",
                            default=r"D:\\OneDrive\\Desktop\\Career\\Internship\\UniKlinikum\\Schapiro\\data\\data\\mask.tif")

    arg_parser.add_argument("--img_path", type=str, help="Path to the image",
                            default=r"D:\\OneDrive\\Desktop\\Career\\Internship\\UniKlinikum\\Schapiro\\data\\data\\s01c1.ome.tif")

    arg_parser.add_argument("--save_dir",
                            type=str, help="Path to save the results",
                            default=r"D:\\OneDrive\\Desktop\\Career\\Internship\\UniKlinikum\\Schapiro\\data\\data\\")

    arg_parser.add_argument("--downsample",
                            help="Downsample", nargs="+", type=float, default=None)

    # 986,2305 and 847,5026

    # [847, 986]

    # [2102, 2446]

    arg_parser.add_argument("--n", type=int,
                            default=1, help="Number of pixels to expand the bounding boxes")

    arg_parser.add_argument("--target_a_ratio",
                            default=1, type=float, help="Aspect ratio to resize the single cells to")

    arg_parser.add_argument("--write-out-my-config",
                            type=str, help="Aspect ratio to resize the single cells to")

    return arg_parser.parse_args()


if __name__ == "__main__":

    # TODO: Add working dir by using Sven's package

    from python_codes.python_logger import get_logger, setup_logging
    from python_codes.parser import CustomArgumentParser

    setup_logging()
    logger = get_logger()

    logger.info("Starting script execution.")

    logger.info("Initialized argument parser.")
    arg_parser = argparse.ArgumentParser()
    # TODO: Save config is not working for some reason

    # # Parse the arguments
    args = get_args(arg_parser)

    logger.info(f"Parsed arguments: {args}")

    logger.info("Calling main function with parsed arguments.")
    df = main(**vars(args))
    logger.info("Main function executed successfully.")

    json_data = json_serialize(df, args.save_dir)
    logger.info("Data serialized to JSON.")
    sys.stdout.write(json_data)
    logger.info("JSON data written to stdout. Exiting script.")
