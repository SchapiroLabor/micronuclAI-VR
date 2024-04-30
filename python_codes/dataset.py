import os.path
import pandas as pd

import tifffile as tf
import numpy as np
from collections import OrderedDict
from vispy.color import Colormap
from napari_tiff import napari_tiff_reader


def checkImageJmetadata(tif_data, keys):
    try:
        meta = tif_data.imagej_metadata
        return [meta[key] for key in keys]
    except:
        return False


# def check_meta(meta, filename_old=None, tif_data=None, img_array=None):
#     if not meta:
#         raise Exception("Please ensure the tif file has metadata")
#     else:

def checkTifTagmetadata(tif_data, index, keys):
    """

    :param tif_data:
    :param index:
    :param keys: keys of frame, channel and slices
    :return:
    """
    return [tif_data.pages[index][key] for key in keys]


#
# def checkSeriesDimension():

def saveaspng(frames=None, img_array=None, filename_old=None, df=None):
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

def tif_to_png(input_file, folder_path, metakeys):

    df = OrderedDict()
    df["frame"] = []
    df["slice"] = []
    df["channel"] = []
    df["name"] = []

    filename_old = os.path.basename(input_file).split(".tif")[0]
    with tf.TiffFile(input_file) as tif_data:
        img_array = tif_data.asarray()
        out = checkImageJmetadata(tif_data, metakeys)
        if out is False:
            if img_array.ndim <3:
                values = checkTifTagmetadata(tif_data, 0, metakeys)
                return


def read_tiff(input_file):

    with tf.TiffFile(input_file) as tif_data:
        data = napari_tiff_reader.tifffile_reader(tif_data)

        array, kwargs, name = data[0]

        if kwargs["colormap"] is None or "gray_r":
            kwargs["colormap"] ="#0000FF"

    return [(array, kwargs, 'image')]










if __name__ == '__main__':
    input_file = "/media/ibrahim/Extended Storage/cloud/Internship/shapiro/cell_tinder/exemplar-001_Ch1_DAPI (1).tif"
    folder_path = "/media/ibrahim/Extended Storage/cloud/Internship/shapiro/data"
    read_tiff(input_file)