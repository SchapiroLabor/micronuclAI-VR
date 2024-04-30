from flask import Flask, request
import python_codes.unity_functions as uf

app = Flask(__name__)

tiff = {}

@app.route("/v1")
def hello_world():
    return "<p>Hello, World!</p>"

@app.route('/v1/tiff_img', methods=['POST', 'GET'])
def get_tiff():
    if request.method == "POST":
        tiff["path"] = request.form["path"]
        return {"Value": "Success"}
    else:
        img, metadata, name = uf.read_tiff(tiff["path"])[0]
        tiff["img"] = img.tolist()
        tiff["metadata"] = metadata
        return tiff







if __name__ == '__main__':
    # run app in debug mode on port 5000
    app.run(debug=True, port=5000)