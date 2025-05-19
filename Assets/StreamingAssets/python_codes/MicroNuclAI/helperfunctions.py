import json
import pandas as pd
from . import logger

def read_from_json(config_file) -> dict:
    import json

    with open(config_file, "r") as f:
        json_args = json.load(f)
    return json_args

def parsejson(data: str | bytes | bytearray) -> dict:
    """
    Parses a JSON-formatted string, bytes, or bytearray and returns the corresponding dictionary.

    Args:
        data (str | bytes | bytearray): The JSON data to parse.

    Returns:
        dict: The parsed JSON data as a Python dictionary.

    Raises:
        json.JSONDecodeError: If the input data is not valid JSON.

    Logs:
        An error message if JSON decoding fails.
    """
        # Attempt to parse the JSON data
    try:
        json_data = json.loads(data)
        return json_data
    except json.JSONDecodeError as e:
        logger.error("Failed to decode JSON: %s", e)


def dataframe2json(df: pd.DataFrame, orient: str = "records") -> str:
    """Convert a pandas DataFrame to JSON format.
    
        Args:
        df: Pandas dataframe

        orient : str
            Indication of expected JSON string format.

            * Series:

                - default is 'index'
                - allowed values are: {{'split', 'records', 'index', 'table'}}.

            * DataFrame:

                - default is 'columns'
                - allowed values are: {{'split', 'records', 'index', 'columns',
                  'values', 'table'}}.

            * The format of the JSON string:

                - 'split' : dict like {{'index' -> [index], 'columns' -> [columns],
                  'data' -> [values]}}
                - 'records' : list like [{{column -> value}}, ... , {{column -> value}}]
                - 'index' : dict like {{index -> {{column -> value}}}}
                - 'columns' : dict like {{column -> {{index -> value}}}}
                - 'values' : just the values array
                - 'table' : dict like {{'schema': {{schema}}, 'data': {{data}}}}

                Describing the data, where data component is like ``orient='records'``.

        Returns
        -------
        None or str
            If path_or_buf is None, returns the resulting json format as a
            string. Otherwise returns None.
    """
    # Convert the DataFrame to JSON
    json_data = df.to_json(orient=orient)
    return json_data


