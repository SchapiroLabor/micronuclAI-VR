import sys
import pandas as pd
import argparse
import json
import pandas as pd
import numpy as np
import os
import re


def escape_special_characters(text):

    raise NotImplementedError("This function is not implemented yet")
    # Define the regex pattern to match special characters
    pattern = r'[\\\"\s(){}<>|:;,?*&#$!`~@]'
    
    # Use re.sub to replace each special character with its escaped version
    escaped_text = re.sub(pattern, lambda m: '\\' + m.group(0), text)

    
    
    escaped_text = escaped_text.replace(" ", "\ ")

    return escaped_text
    

def get_crop_values(data_dir):

    if os.path.exists(data_dir):
        file_path = os.path.join(data_dir, "bbox.csv")
        if not os.path.exists(file_path):
            raise FileNotFoundError(f"bbox not found in {file_path}")

        df = pd.read_csv(os.path.join(data_dir, "bbox.csv"))

        # Drop the 'Unnamed: 0' column if it exists
        if "Unnamed: 0" in df.columns:
            df = df.drop(columns=["Unnamed: 0"])

        # Set 'N' as the index if it exists
        if "N" in df.columns:
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

        # Convert DataFrame to JSON in the required format
        bbox = df.to_json(orient="records")

        return bbox
    else:
        raise FileNotFoundError(f"Directory {data_dir} does not exist")


if __name__ == "__main__":
    print(get_crop_values(sys.argv[-1]))
    exit(0)
