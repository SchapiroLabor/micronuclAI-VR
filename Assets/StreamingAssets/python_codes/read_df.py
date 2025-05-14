import sys
import pandas as pd
import argparse
import json
import pandas as pd
import numpy as np
import os
import re
import logging
from PIL import Image

# Set up logging
log_file = os.path.join(os.path.dirname(os.path.abspath(__file__)), "log.txt")
logging.basicConfig(filename=log_file, level=logging.INFO, format='%(asctime)s - %(levelname)s - %(funcName)s - Line %(lineno)d - %(message)s')
logger = logging.getLogger(__name__)



def get_bbox_from_csv(data_dir) -> pd.DataFrame:
    file_path = os.path.join(data_dir, "bbox.txt")
    if not os.path.exists(file_path):
        raise FileNotFoundError(f"bbox not found in {file_path}")

    df = pd.read_csv(file_path, sep=",", names=["N", "X1", "X2", "Y1", "Y2"])
            # Set 'N' as the index if it exists
    df = df.set_index("N")

    # Replace 'inf' and 'NaN' values with 0
    df.replace([np.inf, -np.inf], 0, inplace=True)
    df.fillna(0, inplace=True)

    # Convert all columns to integer
    for col in df.columns:
        df[col] = df[col].astype(int)

    # Ensure all numerical data is of type float
    for col in df.columns:

        df[col] = df[col].astype(int)

    return bbox

def load_img(data_dir) -> np.ndarray:
    file_path = os.path.join(data_dir, "img.png")

    if not os.path.exists(file_path):
        raise FileNotFoundError(f"img not found in {file_path}")
    img = Image.open(file_path)
    img = np.array(img)

    return img

def crop_img(img, bbox) -> np.ndarray:
    # Assuming bbox is a list of tuples (X1, Y1, X2, Y2)
    crops = []
    for n, (x1, y1, x2, y2) in enumerate(bbox):
        path = os.path.join(data_dir, f"crop_{n}.png")
        
        Image.save(img[y1:y2, x1:x2], f"crop_{n}.png")
    return crops

def img_encode4json(data_dir) -> dict:

    shape = img.shape
    img = img.flatten().tolist()

    df = {"shape": shape, "img": img}
    return df


def get_bbox_from_csv(data_dir) -> pd.DataFrame:
    file_path = os.path.join(data_dir, "bbox.txt")
    if not os.path.exists(file_path):
        raise FileNotFoundError(f"bbox not found in {file_path}")

    df = pd.read_csv(file_path, sep=",", names=["N", "X1", "X2", "Y1", "Y2"])
            # Set 'N' as the index if it exists
    df = df.set_index("N")

    # Replace 'inf' and 'NaN' values with 0
    df.replace([np.inf, -np.inf], 0, inplace=True)
    df.fillna(0, inplace=True)

    # Convert all columns to integer
    for col in df.columns:
        df[col] = df[col].astype(int)

    # Ensure all numerical data is of type float
    for col in df.columns:

        df[col] = df[col].astype(int)

    return bbox


def get_crop_values(data_dir):

    if os.path.exists(data_dir):
        df = get_bbox_from_csv(data_dir)

        # Convert DataFrame to JSON in the required format
        bbox = df.to_dict(orient="list")
        img_df = load_img(data_dir)


        # Works with the following C# format:
        
        #public class DataFrame
        #{
        #    public List<int> X1;
        #    public List<int> X2;
        #    public List<int> Y1;
        #    public List<int> Y2;
        #    public List<float> crop_values_flat_ijk;
        #}

        bbox = json.dumps(bbox)
        return bbox
    else:
        raise FileNotFoundError(f"Directory {data_dir} does not exist")


if __name__ == "__main__":
    name = r"D:\OneDrive\Desktop\Internship\VR_schapiro\data\data"
    data_df = get_crop_values(name)
    sys.stdout.write(data_df)

