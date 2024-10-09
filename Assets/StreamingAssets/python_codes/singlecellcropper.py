import os

import numpy as np
from mask2bbox import BBoxes
import argparse as ap
import tifffile as tiff


def main(args):

    save_dir = os.path.join(args.save_dir)
    if not os.path.exists(save_dir):
        os.makedirs(save_dir)

    # Create a BBoxes object
    all_boxes = BBoxes.from_mask(args.mask_path, args.img_path)

    # Expand the bounding boxes
    all_boxes = all_boxes.expand(n=args.n)

    # Remove the bounding boxes that are located on the edge of the image
    all_boxes = all_boxes.remove_from_edge()

    # Filter the bounding boxes by the sides
    filtered_boxes = all_boxes.filter("sides", np.less_equal,
                                      (args.max_side, args.max_side))

    filtered_boxes.image = tiff.imread(args.img_path)

    # Save your bounding boxes
    np.savetxt(os.path.join(save_dir, "bbox.txt"), filtered_boxes.bboxes,
               delimiter=',', fmt='%d')


    # Get resize factors to resize the bounding boxes to a given size
    resize_factors = filtered_boxes.calculate_resizing_factor(desired_ratio=args.target_a_ratio, size=(args.target_size,
                                                                args.target_size))

    patch_dir = os.path.join(save_dir, "patches")
    if not os.path.exists(patch_dir):
        os.makedirs(patch_dir)

    patch_dir = os.path.join(patch_dir, "img")

    # Extract the bounding boxes as images
    filtered_boxes.extract(resize_factors, size=(args.target_size,
                                                 args.target_size),
                           output=patch_dir, rescale_intensity=True)

# After
if __name__ == "__main__":
    # Create an argument parser
    parser = ap.ArgumentParser(description="Process bounding boxes")

    # Add an argument to the parser
    parser.add_argument("--mask_path", type=str, help="Path to the mask image")
    parser.add_argument("--img_path", type=str, help="Path to the image")
    parser.add_argument("--save_dir",
                        type=str, help="Path to save the results")
    parser.add_argument("--n", type=int,
                        default=10, help="Number of pixels to expand the bounding boxes")
    parser.add_argument("--max_side",
                        default=70, type=int, help="Minimum side of the bounding boxes")
    parser.add_argument("--target_size",
                        default=256, type=int, help="Size to resize the single cells to")
    parser.add_argument("--target_a_ratio",
                        default=0.7, type=float, help="Aspect ratio to resize the single cells to")


    # Parse the arguments
    args = parser.parse_args()

    main(args)