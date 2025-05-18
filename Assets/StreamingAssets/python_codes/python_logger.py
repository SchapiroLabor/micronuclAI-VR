import json
import logging.config
import os
import inspect

# Set up logging
# One child logger per subcomponent, not file.
# Create logging configuration
# Make sure the logger has the correct config
# Use rotating file handler to limit the size of the log file

# Tips: https://www.youtube.com/watch?v=9L77QExPmI0, https://docs.python.org/3/library/logging.html

# TODO Confirm if log file consists of all errors printed out in std.err

def get_logger() -> logging.Logger:
    """Dynamically get the file path of the script that called the function, use the inspect module
    ref: chatgpt 4o"""
    # Get caller frame (1 step up the call stack)
    caller_frame : list = inspect.stack()[1]
    caller_file : str = caller_frame.filename
    logger_name : str = os.path.basename(os.path.dirname(caller_file))
    return logging.getLogger(logger_name)


def setup_logging():
    """Set up logging configuration from a JSON file for root logger. 
    The JSON file should be in the same directory as this script."""

    current_dir : str = os.path.dirname(os.path.abspath(__file__))
    config_file : str = os.path.join(current_dir, "logging_config.json")
    with open(config_file) as f_in:
        config : dict = json.load(f_in)

    logging.config.dictConfig(config)

