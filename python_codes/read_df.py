import pandas as pd
import argparse
import json
import pandas as pd
import numpy as np

def get_crop_values():
    df = pd.read_csv("/media/ibrahim/Extended Storage/OneDrive/Internship/VR_schapiro/repos/cell_tinder/s01c1_bbox.csv")

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


if __name__ == "__main__":
    out = get_crop_values()
    print(out)
