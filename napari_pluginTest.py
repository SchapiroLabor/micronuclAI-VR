# Load image
img = np.moveaxis(tf.imread(r"C:\Schapiro Lab\ibrahim_hiwi\VRproject\VR Demo dataset\image_34.tif"), 0, -1)
viewer = npi.Viewer(title="Test")
viewer.add_image(img, channel_axis=-1)
viewer.show()