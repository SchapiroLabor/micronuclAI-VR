import python_codes.unity_functions as uf
from flask import Flask, request, jsonify

app = Flask(__name__)



tiff = {}


@app.route("/v1")
def hello_world():
    return {"Value": "Success"}


@app.route('/v1/tiff_img', methods=['POST', 'GET'])
def get_tiff():
    if request.method == "POST":
        try:
            tiff["path"] = request.json["path"]
            return tiff
        except Exception as e:
            print("Got a problem: {} \n Caused by: {}".format(e, request.data))
            return e


    else:
        try:
            array, kwargs, shape, dtype_string = uf.fluorescent_channel2rgb(tiff["path"])
            tiff["img"] = [x.flatten().tolist() for x in array]  # Integrity is kept intact
            print(len(tiff["img"]))
            tiff["metadata"] = kwargs
            tiff["shape"] = shape
            print(tiff["shape"])
            tiff["dtype"] = dtype_string
            #print("First array of RGB uint16 image: {}".format(tiff["img"][0][:6]))
            return tiff #tiff
        except Exception as e:
            print("Got a problem here: {}".format(e))
            return e


if __name__ == '__main__':
    # run app in debug mode on port 5000
    app.run(debug=True, port=5000)
