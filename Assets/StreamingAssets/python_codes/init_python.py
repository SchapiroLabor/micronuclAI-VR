import venv
import os
import sys


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
    venv_directory = sys.argv[-1]

    venv_directory = os.path.join(venv_directory, "MNAIVR")
    python = os.path.join(venv_directory, 'Scripts', 'python.exe')
    if not os.path.exists(python):
        python = create_venv_and_get_executable(venv_directory)

    # List packages needing to install
    # Does not work.
    # TODO Execute automatic install of packages. For now, make user install manually.
    import subprocess

    required = {'pandas', 'tifffile', 'mask2bbox', 'numpy', "Flask"}
    subprocess.check_call([python, '-m', 'pip', 'install', '--no-input', *required])

    sys.stdout.write(python)


    


