import os
from . import args_parser
from . import logger
import numpy as np
from mask2bbox import BBoxes
import tifffile as tiff


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

    logger.info(f"Creating save directory at {save_dir} if it doesn't exist.")
    save_dir: str = os.path.join(save_dir)
    if not os.path.exists(save_dir):
        os.makedirs(save_dir)
        logger.info(f"Directory created: {save_dir}")
    else:
        logger.info(f"Directory already exists: {save_dir}")

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

    bbox_path: str = os.path.join(save_dir, "bbox.txt")
    logger.info(f"Saving bounding boxes to {bbox_path}.")
    np.savetxt(bbox_path, filtered_boxes.bboxes,
               delimiter=',', fmt='%d')

    logger.info(f"Calculating resize factors for target size {target_size} and aspect ratio {target_a_ratio}.")
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

    main(args)