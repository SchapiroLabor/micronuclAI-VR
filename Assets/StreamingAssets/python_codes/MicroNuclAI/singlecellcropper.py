import os
from typing import Any
import numpy as np
from mask2bbox._bboxes import BBoxes
import tifffile as tiff
import sys
import pandas as pd
import json
import pandas as pd
import numpy as np
import os
from PIL import Image
import argparse
from helperfunctions import save2DFcolumn
sys.path.append(os.path.dirname(os.path.dirname(
    os.path.dirname(os.path.abspath(__file__)))))


def main(results_dir: str,
         mask_path: str,
         img_path: str,
         n: int,
         target_a_ratio: float, **kwargs: dict
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

    csv_path = os.path.join(results_dir, "bbox.csv")

    if not os.path.exists(results_dir) or not [s for s in os.listdir(results_dir) if s.endswith(".png")] \
            or not os.path.exists(csv_path):

        os.makedirs(results_dir, exist_ok=True)

        logger.info(f"Directory created: {results_dir}")

        logger.info("Creating BBoxes object from mask and image.")
        all_boxes: BBoxes = BBoxes.from_mask(mask_path, img_path)

        logger.info(f"Expanding bounding boxes by {n} pixels.")
        all_boxes: BBoxes = all_boxes.expand(n=n)

        logger.info("Removing bounding boxes located on the edge of the image.")
        all_boxes: BBoxes = all_boxes.remove_from_edge()

        sides: np.ndarray = all_boxes.get_sides()[1:, ...]

        third_quartile_cols: int = int(np.quantile(sides[:, 0], 0.95))
        third_quartile_rows: int = int(np.quantile(sides[:, 1], 0.95))

        logger.info(
            f"Filtering bounding boxes with sides <= ({third_quartile_cols}, {third_quartile_rows}).")
        all_boxes: BBoxes = all_boxes.filter("sides", np.less_equal,
                                             (third_quartile_cols, third_quartile_rows))

        logger.info(f"Reading image from {img_path}.")
        all_boxes.image = tiff.imread(img_path)
        img_path = os.path.join(results_dir, "img.png")
        save_img(all_boxes.image, img_path, 'PNG')

        target_size: int = int(np.median(sides.flatten()))

        resize_factors: np.ndarray = all_boxes.calculate_resizing_factor(
            desired_ratio=target_a_ratio, size=(target_size, target_size))

        patch_dir: str = os.path.join(results_dir, "patches")
        if not os.path.exists(patch_dir):
            os.makedirs(patch_dir)
            logger.info(f"Patch directory created: {patch_dir}")
        else:
            logger.info(f"Patch directory already exists: {patch_dir}")

        crop_path: str = os.path.join(patch_dir, "img")

        all_boxes.extract(resize_factors, size=(target_size,
                                                target_size),
                          output=crop_path, rescale_intensity=True)
        logger.info("Extraction and saving of patches completed.")

        bbox_path: str = os.path.join(results_dir, "bbox.csv")

        logger.info(f"Saving bounding boxes to {bbox_path}.")

        df = pd.DataFrame(all_boxes.bboxes, columns=[
            "label_ids", "X1", "X2", "Y1", "Y2"])

        df = df.astype(int)

        df["patch_path"] = df["label_ids"].apply(
            lambda k: os.path.join(f"{crop_path}_{k}.png"))

        df[f"whole_well_img_ndim"] = all_boxes.image.ndim
        df["whole_well_img_path"] = img_path

        logger.info(
            f"Dims: {all_boxes.image.ndim} and shape: {all_boxes.image.shape}")

        dim_dict = {0: "Y", 1: "X", 3: "C", 2: "Z", 4: "T"}

        for dim in range(all_boxes.image.ndim):
            value = dim_dict[dim]
            df[f"whole_well_img_shape_{value}"] = all_boxes.image.shape[dim]

        # Save to CSV
        df.to_csv(csv_path, index=False)
    else:
        logger.info(f"Directory already exists: {results_dir}")
        # Read the existing CSV file
        df = pd.read_csv(csv_path)
    return df


def save_img(img, file_path: str, format: str = 'PNG'):
    if "tif" in os.path.splitext(file_path)[-1]:
        tiff.imwrite(file_path, img)
    else:
        Image.fromarray(img).save(file_path, format=format)


def get_bbox_from_csv(data_dir) -> pd.DataFrame:
    file_path = os.path.join(data_dir, "bbox.txt")

    if not os.path.exists(file_path):
        raise FileNotFoundError(f"bbox not found in {file_path}")

    df = pd.read_csv(file_path, sep=",", names=[
                     "label_ids", "X1", "X2", "Y1", "Y2", "whole_well_img_shape"])
    # Set 'N' as the index if it exists
    df = df.set_index("N")

    # Replace 'inf' and 'NaN' values with 0
    sanitize_dataframe(df)

    # Convert all columns to integer in a vectorized way
    df = df.astype(int)

    return df


def load_img(data_dir) -> np.ndarray:
    file_path = os.path.join(data_dir, "img.png")

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


def get_args(arg_parser: argparse.ArgumentParser) -> argparse.Namespace:
    # Add an argument to the parser
    arg_parser.add_argument("--mask_path", type=str,
                            help="Path to the mask image",
                            default=r"D:\\OneDrive\\Desktop\\Career\\Internship\\UniKlinikum\\Schapiro\\data\\data\\mask.tif")

    arg_parser.add_argument("--img_path", type=str, help="Path to the image",
                            default=r"D:\\OneDrive\\Desktop\\Career\\Internship\\UniKlinikum\\Schapiro\\data\\data\\s01c1.ome.tif")

    arg_parser.add_argument("--save_dir",
                            type=str, help="Path to save the results",
                            default=r"D:\\OneDrive\\Desktop\\Career\\Internship\\UniKlinikum\\Schapiro\\data\\data\\")

    arg_parser.add_argument("--n",
                            default=15, type=int, help="Buffer to nucleus mask")

    arg_parser.add_argument("--target_a_ratio",
                            default=0.7, type=float, help="Aspect ratio to resize the single cells to")

    arg_parser.add_argument("--write-out-my-config",
                            type=str, help="Aspect ratio to resize the single cells to")

    return arg_parser.parse_args()


if __name__ == "__main__":

    # TODO: Add working dir by using Sven's package

    from python_codes.python_logger import get_logger, setup_logging
    from python_codes.parser import CustomArgumentParser

    setup_logging()
    logger = get_logger()

    arg_parsered: argparse.ArgumentParser = CustomArgumentParser.get_arg_parser()

    # # Parse the arguments
    args: argparse.Namespace = get_args(arg_parsered)

    logger.info(f"Arguments 1: {args}")

    # TODO: Add accessible args string for wrtie out config
    args: argparse.Namespace = arg_parsered.set_namespace_from_config(args,
                                                                      args.write_out_my_config)

    logger.info(f"Arguments 2: {args}")

    config: dict[str, Any] = arg_parsered.namespace2dict(args)

    df: pd.DataFrame = main(results_dir=args.save_dir, **config)

    # Save to config python file
    filtered_df = df.loc[:, [x for x in df.columns if x.startswith(
        "whole_well_img")]].drop_duplicates()

    df = df.loc[:, ~df.columns.isin(filtered_df.columns)]

    config_df: dict[str, Any] = filtered_df.to_dict(orient="records")

    [config.update(d) for d in config_df]

    # Oversave config with the new values
    args: argparse.Namespace = arg_parsered.set_namespace_from_dict(
        args, config)

    logger.info(f"Arguments 3: {args}")

    arg_parsered.save_config(args, configargparse_filter=False)

    # df = pd.DataFrame({
    #     "id": [1, 2],
    #     "tags": [[1, 2], [1, 2]]
    # })

    json_data = json_serialize(df, args.save_dir)

    sys.stdout.write(json_data)
