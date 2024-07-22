import pandas as pd
import argparse

parser = argparse.ArgumentParser(description="Get crop values for a given cell index.")
parser.add_argument("--cell_index", type=int, default=1, help="Cell index to retrieve crop values for")
args = parser.parse_args()

def get_crop_values(cell_index):
    df = pd.read_csv("/media/ibrahim/Extended Storage/OneDrive/Internship/VR_schapiro/repos/cell_tinder/s01c1_bbox.csv")
    bbox = df[df["N"] == cell_index].to_json(orient="records")
    return bbox


if __name__ == "__main__":
    out = get_crop_values(args.cell_index)
    print(out)
