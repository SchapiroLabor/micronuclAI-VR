from python_codes.dataset import read_tiff
import numpy as np


def fluorescent_channel2rgb(tiff_path=None):
    # Read tiff using napari plug in
    array, kwargs, name = read_tiff(tiff_path)[0]  # Open list
    shape = array.shape
    # TODO automate rgb img creation for fluorescent channel
    #if kwargs["colormap"] == "#0000FF":
    array = np.stack([np.zeros(shape), np.zeros(shape), array], axis=-1).astype(array.dtype)
    shape = array.shape
    return array, kwargs, shape, str(array.dtype)
