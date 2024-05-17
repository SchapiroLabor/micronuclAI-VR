from python_codes.dataset import read_tiff
import numpy as np


def fluorescent_channel2rgb(tiff_path=None):
    # Read tiff using napari plug in
    array, kwargs, name = read_tiff(tiff_path)[0]  # Open list
    shape = array.shape
    array = array.astype(np.uint16)
    axes = np.sort(shape)
    c_indx = np.argwhere(np.array(shape) == axes[0])
    if 0 in c_indx:
        array = np.transpose(array, (1, 2, 0))

    if axes[0] > 3:
        array = np.split(array, np.arange(3, axes[0], 3), axis=-1)[0]
    else:
        [array] = array
    shape = array[0].shape
    # TODO automate rgb img creation for fluorescent channel
    #if kwargs["colormap"] == "#0000FF":
    #array = np.stack([np.zeros(shape), np.zeros(shape), array], axis=-1).astype(array.dtype)
    #shape = array.shape
    return array, kwargs, shape, str(array[0].dtype)
