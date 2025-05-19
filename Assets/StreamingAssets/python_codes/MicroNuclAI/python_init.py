import venv
import os
import sys
from . import arg_parser


def create_venv_and_get_executable(venv_dir):
    # Create the virtual environment
    venv.create(venv_dir, with_pip=True)

    # Determine the path to the Python executable in the virtual environment
    if os.name == 'nt':  # Windows
        python_executable = os.path.join(venv_dir, 'Scripts', 'python.exe')

    else:  # macOS/Linux
        python_executable = os.path.join(venv_dir, 'bin', 'python')

    return python_executable


if __name__ == "__main__":

    # Example usage. Should be saved in StreamingAssets
    # Assume this is a directory in the Unity project
    venv_directory = sys.argv[-1]

    venv_directory = os.path.join(venv_directory, "MNAIVR")

    python = os.path.join(venv_directory, 'Scripts', 'python.exe')

    if not os.path.exists(python):
        python = create_venv_and_get_executable(venv_directory)

    # List packages needing to install
    # Does not work.
    # TODO Execute dynamic install of packages. For now, specify the needed packages manually.
    import subprocess

    required = {'pandas', 'tifffile', 'mask2bbox',
                'numpy', "Flask", "pyyaml", "ConfigArgParse"}
    subprocess.check_call(
        [python, '-m', 'pip', 'install', '--no-input', *required])

    arg_parser.variables_dict["MNAIVR_exe"] = python

    # Write config file. I fear that printing to stout will mess with logging of
    # the python thread as we have logs printed out in the standard output file.
