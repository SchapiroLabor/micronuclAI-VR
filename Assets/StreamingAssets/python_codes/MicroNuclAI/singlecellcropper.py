import os
from . import args_parser
from . import logger
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


def main(save_dir: str,
        mask_path: str,
        img_path: str,
        n: int,
        max_side: int,
        target_size: int,
        target_a_ratio: float
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

    if not os.path.exists(save_dir) or not [s for s in os.listdir(save_dir) if s.endswith(".png")] \
    or not os.path.exists(os.path.join(save_dir, "bbox.csv")):
        os.makedirs(save_dir)
        logger.info(f"Directory created: {save_dir}")

        logger.info("Creating BBoxes object from mask and image.")
        all_boxes: BBoxes = BBoxes.from_mask(mask_path, img_path)

        logger.info(f"Expanding bounding boxes by {n} pixels.")
        all_boxes: BBoxes = all_boxes.expand(n=n)

        logger.info("Removing bounding boxes located on the edge of the image.")
        all_boxes: BBoxes = all_boxes.remove_from_edge()

        logger.info(f"Filtering bounding boxes with sides <= ({max_side}, {max_side}).")
        filtered_boxes: BBoxes = all_boxes.filter("sides", np.less_equal,
                                                (max_side, max_side))

        logger.info(f"Reading image from {img_path}.")
        filtered_boxes.image = tiff.imread(img_path)

        logger.info(f"Calculating resize factors for target size {target_size} and \
                    aspect ratio {target_a_ratio}.")
        resize_factors: np.ndarray = filtered_boxes.calculate_resizing_factor(
            desired_ratio=target_a_ratio, size=(target_size, target_size))

        patch_dir: str = os.path.join(save_dir, "patches")
        if not os.path.exists(patch_dir):
            os.makedirs(patch_dir)
            logger.info(f"Patch directory created: {patch_dir}")
        else:
            logger.info(f"Patch directory already exists: {patch_dir}")

        patch_dir: str = os.path.join(patch_dir, "img")

        logger.info(f"Extracting and saving patches to {patch_dir}.")
        filtered_boxes.extract(resize_factors, size=(target_size,
                                                    target_size),
                            output=patch_dir, rescale_intensity=True)
        logger.info("Extraction and saving of patches completed.")

        bbox_path: str = os.path.join(save_dir, "bbox.csv")
        logger.info(f"Saving bounding boxes to {bbox_path}.")
        df = pd.DataFrame(filtered_boxes.bboxes, columns=["N", "X1", "X2", "Y1", "Y2"])
        df = df.astype(int)
        df["img_path"] = df["N"].apply(lambda x: os.path.join(patch_dir, f"img_{x}.png"))
        df["whole_slide_img_shape"] = filtered_boxes.image.shape
        # Save to CSV
        df.to_csv("output.csv", index=False)
    else:
        logger.info(f"Directory already exists: {save_dir}")
        # Read the existing CSV file
        df = pd.read_csv(os.path.join(save_dir, "bbox.csv"))

    return df

    


def get_bbox_from_csv(data_dir) -> pd.DataFrame:
    file_path = os.path.join(data_dir, "bbox.txt")
    
    if not os.path.exists(file_path):
        raise FileNotFoundError(f"bbox not found in {file_path}")

    df = pd.read_csv(file_path, sep=",", names=["N", "X1", "X2", "Y1", "Y2"])
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

def img_encode4json(data_dir) -> dict:

    shape = img.shape
    img = img.flatten().tolist()

    df = {"shape": shape, "img": img}
    return df

def sanitize_dataframe(df: pd.DataFrame) -> pd.DataFrame:

    if [np.inf, -np.inf] in df.values:
        logger.warning("DataFrame contains infinite values. Replacing with 0.")
        df.replace([np.inf, -np.inf], 0, inplace=True) # In memory replacement
    
    if df.isnull().values.any():
        logger.warning("DataFrame contains NaN values. Replacing with 0.")
        # Replace NaN values with 0
        df.fillna(0, inplace=True) # In memory replacement


def json_serialize(df, data_dir):

    if os.path.exists(data_dir):

        # Convert DataFrame to JSON in the required format
        bbox = df.to_dict(orient="list")
        #img_df = load_img(data_dir)


        # Works with the following C# format:
        
        #public class DataFrame
        #{   public List<int> N;
        #    public List<int> X1;
        #    public List<int> X2;
        #    public List<int> Y1;
        #    public List<int> Y2;
        #    public List<int[]> img;
        #}

        bbox = json.dumps(bbox)
        return bbox
    else:
        raise FileNotFoundError(f"Directory {data_dir} does not exist")

def get_args():
    # Add an argument to the parser
    args_parser.add_argument("--mask_path", type=str, help="Path to the mask image")

    args_parser.add_argument("--img_path", type=str, help="Path to the image")

    args_parser.add_argument("--save_dir",
                        type=str, help="Path to save the results")
    
    args_parser.add_argument("--n", type=int,
                        default=10, help="Number of pixels to expand the bounding boxes")
    
    args_parser.add_argument("--max_side",
                        default=70, type=int, help="Minimum side of the bounding boxes")
    
    args_parser.add_argument("--target_size",
                        default=256, type=int, help="Size to resize the single cells to")
    
    args_parser.add_argument("--target_a_ratio",
                        default=0.7, type=float, help="Aspect ratio to resize the single cells to")
    
    return args_parser.parse_args()

if __name__ == "__main__":

    # Parse the arguments
    args = get_args()

    df = main(args)

    json_data = json_serialize(df, args.save_dir)

    sys.stdout.write(json_data)