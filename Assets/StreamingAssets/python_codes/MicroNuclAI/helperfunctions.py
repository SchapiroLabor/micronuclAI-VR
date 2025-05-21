import json
import pandas as pd
import sys
import numpy as np


def save2DFcolumn(
    source_ids: list,
    source_col: str,
    target_ids: np.ndarray,
    dataframe: pd.DataFrame,
    target_col: str = "Embedding"
) -> pd.DataFrame:
    """
    Adds a new column to the dataframe with values from sorted_results,
    mapped according to sorted_nucl_labels.

    Parameters:
    - sorted_results: List of values to be added as the new column.
    - sorted_nucl_labels: 1D or 2D numpy array of nucleus labels.
    - dataframe: The DataFrame to which the new column will be added.
    - column_name: The name of the new column (default is "Embedding").

    Returns:
    - Updated DataFrame with the new column.
    """

    if isinstance(source_ids, np.ndarray):

        if source_ids.ndim > 2:
            raise NotImplementedError(
                "Handling for multi-column sorted_nucl_labels is not implemented.")
        else:
            source_ids = source_ids.tolist()

    if not isinstance(target_ids, np.ndarray):
        target_ids = np.array(target_ids)
    # Flatten sorted_nucl_labels if it has only one column (2D array with shape [n, 1])
    if target_ids.ndim == 2:
        if target_ids.shape[0] >= 1 and target_ids.shape[1] == 1:
            target_ids = target_ids.flatten()

        if len(source_ids) > len(target_ids) and len(target_ids) == 1:
            # If sorted_results is a single value, repeat it for each entry in sorted_nucl_labels
            target_ids = target_ids*len(source_ids)

        else:
            raise ValueError(
                "More than one row in sorted_nucl_labels, but only one column is expected.")

    # Create a dictionary for fast lookup of results by label
    label_to_result = dict(zip(source_ids, target_ids))

    # Map each NUCLEUS_LABEL_KEY in the dataframe to its corresponding result, or NaN if not found
    dataframe[target_col] = dataframe[source_col].map(
        label_to_result).fillna(np.nan)

    return dataframe


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
        sys.stderr.write("Failed to decode JSON: %s", e)


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
