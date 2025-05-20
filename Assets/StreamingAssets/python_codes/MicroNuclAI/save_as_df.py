import sys
import os
import pandas as pd
from .helperfunctions import parsejson, read_from_json
from . import logger


def readtxt(cwd) -> dict:
    logger.info("Reading from text file")
    path = os.path.join(cwd, "message.txt")
    # Read json from txt file
    with open(path, "r") as f:
        data = f.read()
    return parsejson(data)


def readfromstdin() -> dict:
    logger.info("Reading from standard input")
    # This method takes way too long to read from stdin input
    # Read from stdin
    data = sys.stdin.read()
    return parsejson(data)


def read_from_pipe():
    # Convert the pipe handle from a string to an integer
    pipe_fd = int(sys.argv[-1])

    logger.info("Reading from pipe with handle: %s", pipe_fd)

    # Read from the pipe
    # This method does not take C# ClientPipeHandle as an argument
    with os.fdopen(pipe_fd, 'r') as pipe:
        # Read the entire content from the pipe
        data = pipe.read()


def save_as_df(json_data, data_dir):

    try:
        # Convert the JSON data to a pandas DataFrame
        logger.info(f"Converting JSON file to DataFrame")
        df = pd.DataFrame(json_data)
        df.to_csv(os.path.join(data_dir, "output.csv"))

    except Exception as e:
        logger.error("Failed to convert JSON data to DataFrame: %s", e)


if __name__ == "__main__":
    # The pipe handle is passed as the first argument
    logger.info("Python process started with arguments: %s", sys.argv)

#    json_data_path = sys.argv[-1]

#   json_data = read_from_json(json_data_path)

#   save_as_df(json_data, os.path.dirname(json_data_path))

    sys.stdout.write("Python process started with arguments: %s\n" % sys.argv)

    # Log to standard output file, so to have complete log from all processes
