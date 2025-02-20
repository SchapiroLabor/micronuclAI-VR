import python_codes.unity_functions as uf
<<<<<<< Updated upstream
from flask import Flask, request, jsonify
import napari as npi
import tifffile as tf
import numpy as np
=======
from flask import Flask, request
import napari as npi
import tifffile as tf
import numpy as np


>>>>>>> Stashed changes
app = Flask(__name__)
# img = np.moveaxis(tf.imread(r"/media/ibrahim/Extended Storage/cloud/Internship/shapiro/exemplar-001_Ch1_DAPI.tif"), 0, -1)
# viewer = npi.Viewer(title="Test", show=True)
# viewer.add_image(img[...,None], channel_axis=-1)
# viewer.window._qt_viewer.canvas.show(visible=False, run=False)


tiff = {}

@app.route("/v1")
def hello_world():
    return {"Value": "Success"}


@app.route('/v1/tiff_img', methods=['POST', 'GET'])
def get_tiff():
    if request.method == "POST":
        try:
            tiff["img"] = request.json["img"]
            tiff["shape"] = request.json["shape"]
            tiff["dtype"] = request.json["dtype"] #dtype_string
            tiff["channel_sizes"] = request.json["channel_sizes"]  # dtype_string
            return tiff
        except Exception as e:
            print("Got a problem: {} \n Caused by: {}".format(e, request.data))
            return e


    else:
        try:
            #array, kwargs, shape, dtype_string = uf.fluorescent_channel2rgb(tiff["path"])

            #tiff["img"] = img.flatten() #[x.flatten().tolist() for x in array]  # Integrity is kept intact
            #tiff["metadata"] = kwargs

            #print("First array of RGB uint16 image: {}".format(tiff["img"][0][:6]))
            return tiff #tiff
        except Exception as e:
            print("Got a problem here: {}".format(e))
            return e


@app.route('/v1/napari_test', methods=['GET'])
def napari2VR():
    # if request.method == "POST":
    #     try:
    #         if request.json["command"] == "Loadin2VR":
    #             # Start VR
    #             # Return IMG
    #
    #
    #         return tiff
    #     except Exception as e:
    #         print("Got a problem: {} \n Caused by: {}".format(e, request.data))
    #         return e



    try:

        img = np.moveaxis(tf.imread(r"C:\Schapiro Lab\ibrahim_hiwi\VRproject\VR Demo dataset\image_34.tif"), 0, -1)
        viewer = npi.Viewer(title="Test")
        viewer.add_image(img, channel_axis=-1)
        viewer.show()

        array = viewer.screenshot()
        tiff["img"] = array.flatten()  # Integrity is kept intact
        tiff["shape"] = array.shape[:-1]
        tiff["dtype"] = str(array.dtype)
        tiff["channels"] = array.shape[-1]

        print("First array of RGB uint16 image: {}".format(tiff["img"][0][:6]))
        return tiff #tiff
    except Exception as e:
        print("Got a problem here: {}".format(e))
        return e



if __name__ == '__main__':
    # run app in debug mode on port 5000
    app.run(debug=True, port=5000)
