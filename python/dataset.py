import os.path
import pandas as pd
import tifffile as tf
from PIL import Image
import numpy as np
from collections import OrderedDict
import cv2
def check_meta(meta, filename_old=None, tif_data=None, img_array=None):
    if not meta:
        raise Exception("Please ensure the tif file has metadata")
    else:
        df = OrderedDict()
        df["frame"] = []
        df["slice"] = []
        df["channel"] = []
        df["name"] = []
        print(meta.items())
        channels, slices, frames = meta["channels"], meta["slices"], meta["frames"]
        print("Converting tif from data type {} to {}".format(tif_data.dtype, np.uint8))
        if frames == 1: img_array = np.expand_dims(img_array, axis=0)
        if channels == 1: img_array = np.expand_dims(img_array, axis=-1)
        if slices == 1: img_array = np.expand_dims(img_array, axis=1)
        img_array = [[np.split(slicey, channels, axis=1) for slicey in
                      np.split(frame, slices, axis=1)] for frame in
                     np.split(img_array, frames, axis=0)]

        for frame in range(frames):
            for slicey in slices:
                for channel in channels:
                    filename = filename_old + "_frame_{}_slice_{}_channel_{}.png".format(frame, slicey, channel)
                    img = img_array[frame, slicey, channel].astype(np.uint8)
                    df["frame"] += [frame],
                    df["slice"] += [slicey],
                    df["channel"] += [channel]
                    df["name"] += [filename_old]
                    img = Image.fromarray(img, mode="L")
                    img.save(os.path.join(folder_path, filename), 'PNG')
        pd.DataFrame(df).to_csv(os.path.join(folder_path, "metadata"))

def tif_to_png(input_file, folder_path):

    filename_old = os.path.basename(input_file).split(".tif")[0]
    with tf.TiffFile(input_file) as tif_data:
        meta = tif_data.imagej_metadata
        img_array = tif_data.asarray()
        print("Converting tif from data type {} to {}".format(img_array.dtype, np.uint8))
        if img_array.ndim <3:
            #img = img_array/256 #.astype(np.uint8, casting="same_kind")
            #img = Image.fromarray(img_array, mode="L")
            filename = filename_old + "_frame_{}_slice_{}_channel_{}.png".format(1, 1, 1)
            cv2.imwrite(filename, img_array)
            #img.save(os.path.join(folder_path, filename), 'PNG')
        else:
            check_meta(meta, filename_old=None, tif_data=None, img_array=None)



if __name__ == '__main__':
    input_file = "/media/ibrahim/Extended Storage/cloud/Internship/shapiro/cell_tinder/exemplar-001_Ch1_DAPI (1).tif"
    folder_path = "/media/ibrahim/Extended Storage/cloud/Internship/shapiro/data"
    tif_to_png(input_file, folder_path)