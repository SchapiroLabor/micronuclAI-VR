from python_codes.dataset import read_tiff
import numpy as np

def create2DTexture(input_file="/media/ibrahim/Extended Storage/cloud/Internship/shapiro/cell_tinder/exemplar-001_Ch1_DAPI (1).tif"):
    array, kwargs, name = read_tiff(input_file)[0]
    shape = array.shape

    if kwargs["colormap"] == "#0000FF":
        array = np.array([np.zeros(shape), np.zeros(shape), array]).astype(np.uint16)

    return array
