import sys
import os
import json
import pandas as pd
import logging

# Set up logging
log_file = os.path.join(os.path.dirname(os.path.abspath(__file__)), "log.txt")
logging.basicConfig(filename=log_file, level=logging.INFO, format='%(asctime)s - %(levelname)s - %(funcName)s - Line %(lineno)d - %(message)s')
logger = logging.getLogger(__name__)

data_dir = os.path.join(os.path.dirname(os.path.abspath(__file__)), "results")

if not os.path.exists(data_dir):
    os.makedirs(data_dir)

def read_from_pipe():
    # Convert the pipe handle from a string to an integer
    pipe_fd = int(sys.argv[-1])

    logger.info("Reading from pipe with handle: %s", pipe_fd)

    # Read from the pipe
    # This method does not take C# ClientPipeHandle as an argument
    with os.fdopen(pipe_fd, 'r') as pipe:
        # Read the entire content from the pipe
        data = pipe.read()
        
        # Attempt to parse the JSON data
        try:
            json_data = json.loads(data)
            return json_data
        except json.JSONDecodeError as e:
            logger.error("Failed to decode JSON: %s", e)

def save_as_df(json_data):

    try:
        # Convert the JSON data to a pandas DataFrame
        logger.info(f"Converting JSON data {json_data} to DataFrame")

        df = pd.DataFrame(json_data)
        df.to_csv(os.path.join(data_dir, "output.csv"))
    except Exception as e:
        logger.info("Failed to convert JSON data to DataFrame: %s", e)

def readfromstdin():
    logger.info("Reading from standard input")
    # This method takes way too long to read from stdin input
    # Read from stdin
    data = sys.stdin.read()

    # Attempt to parse the JSON data
    try:
        json_data = json.loads(data)
        return json_data
    except json.JSONDecodeError as e:
        logger.error("Failed to decode JSON: %s", e)

def readtxt():
    logger.info("Reading from text file")
    path = os.path.join(os.path.dirname(data_dir), "message.txt")
    # Read json from txt file
    with open(path, "r") as f:
        data = f.read()
    # Attempt to parse the JSON data
    try:
        json_data = json.loads(data)
        return json_data
    except json.JSONDecodeError as e:
        logger.error("Failed to decode JSON: %s", e)

if __name__ == "__main__":
    # The pipe handle is passed as the first argument
    logger.info("Python process started with arguments: %s", sys.argv)
    data = readtxt()
    save_as_df(data)
    exit(0)
