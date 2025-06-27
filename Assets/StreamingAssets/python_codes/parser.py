import os
import inspect
import argparse
import types
import glob
import sys
import configargparse
import json

default_path = os.path.join(os.path.dirname(os.path.abspath(__file__)),
                            "python_config.yml")


def load_config_file(config_file_path: str, k: str = "r") -> dict:
    """Load a configuration file from the given path.

    Args:
        config_file_path (str): Path to the configuration file.

    Returns:
        dict: Parsed configuration data.
    """
    if not os.path.exists(config_file_path):

        if config_file_path.endswith('.yaml'):
            import yaml
            with open(config_file_path, k) as file:
                config_data = yaml.safe_load(file)

        elif config_file_path.endswith('.json'):
            with open(config_file_path, k) as file:
                config_data = json.load(file)
        else:
            raise ValueError(
                "Unsupported file format. Please use .yaml or .json files.")

        return config_data


class CustomArgumentParser(configargparse.ArgumentParser):

    """

    Custom argument parser that extends configargparse.ArgumentParser to handle configuration 
    files and command-line arguments.

    ref: https://github.com/bw2/ConfigArgParse

    """

    def __init__(self, ignore_unknown_config_file_keys=True,
                 default_config_files=[default_path],
                 **kwargs):

        # TODO Confirm if unification of multiple instances to single config is feasible

        super().__init__(ignore_unknown_config_file_keys=ignore_unknown_config_file_keys,
                         default_config_files=default_config_files,
                         **kwargs)

        self.add_argument("-w", "--write-out-my-config", required=False, is_write_out_config_file_arg=False,
                          default=default_config_files[0],
                          help='write out config file path')

        self.variables_dict = {}

        self.arguments_key = "arguments"
        self.variables_key = "variables"

        # self._config_file_open_func = json.load

    def _open_config_files(self, command_line_args):
        """Tries to parse config file path(s) from within command_line_args.
        Returns a list of opened config files, including files specified on the
        commandline as well as any default_config_files specified in the
        constructor that are present on disk.

        Args:
            command_line_args: List of all args

        Returns:
            list[IO]: open config files
        """
        # open any default config files

        try:
            config_files = []
            for files in map(glob.glob, map(os.path.expanduser, self._default_config_files)):
                for f in files:
                    config_files.append(self._config_file_open_func(f)[
                                        self.arguments_key])

        except Exception as e:
            sys.stderr.write(
                "Unable to open default config file(s): %s. Error: %s\n" % (
                    self._default_config_files, str(e)))

        # list actions with is_config_file_arg=True. Its possible there is more
        # than one such arg.
        user_config_file_arg_actions = [
            a for a in self._actions if getattr(a, "is_config_file_arg", False)]

        if not user_config_file_arg_actions:
            return config_files

        for action in user_config_file_arg_actions:
            # try to parse out the config file path by using a clean new
            # ArgumentParser that only knows this one arg/action.
            arg_parser = argparse.ArgumentParser(
                prefix_chars=self.prefix_chars,
                add_help=False)

            arg_parser._add_action(action)

            # make parser not exit on error by replacing its error method.
            # Otherwise it sys.exits(..) if, for example, config file
            # is_required=True and user doesn't provide it.
            def error_method(self, message):
                pass
            arg_parser.error = types.MethodType(error_method, arg_parser)

            # check whether the user provided a value
            parsed_arg = arg_parser.parse_known_args(args=command_line_args)
            if not parsed_arg:
                continue
            namespace, _ = parsed_arg
            user_config_file = getattr(namespace, action.dest, None)

            if not user_config_file:
                continue

            # open user-provided config file
            user_config_file = os.path.expanduser(user_config_file)
            try:
                stream = self._config_file_open_func(user_config_file)
            except Exception as e:
                if len(e.args) == 2:  # OSError
                    errno, msg = e.args
                else:
                    msg = str(e)
                # close previously opened config files
                for config_file in config_files:
                    try:
                        config_file.close()
                    except Exception:
                        pass
                self.error("Unable to open config file: %s. Error: %s" % (
                    user_config_file, msg
                ))

            config_files += [stream]

        return config_files

    def ordereddict2dict(self, od):
        """Convert an OrderedDict to a regular dict."""

        if isinstance(od, dict):
            return {k: self.ordereddict2dict(v) for k, v in od.items()}
        elif isinstance(od, list):
            return [self.ordereddict2dict(item) for item in od]
        else:
            return od

    def write_config_file(self, parsed_namespace, output_file_paths: list, exit_after=False):
        """Write the given settings to output files.

        Args:
            parsed_namespace: namespace object created within parse_known_args()
            output_file_paths: any number of file paths to write the config to
            exit_after: whether to exit the program after writing the config files
        """
        default_path = None
        for output_file_path in output_file_paths:
            # validate the output file path
            try:
                with self._config_file_open_func(output_file_path, "w") as output_file:
                    pass
            except:
                if output_file_paths.__len__() == 1:
                    default_path = output_file_paths[0]
                output_file_paths.remove(output_file_path)

        if not output_file_paths:
            if default_path is None:
                return
            output_file_paths = [default_path]

        # generate the config file contents
        self.arguments_dict = self.get_items_for_config_file_output(
            self._source_to_settings, parsed_namespace)

        config_items = {self.arguments_key: self.ordereddict2dict(self.arguments_dict),
                        self.variables_key: self.variables_dict}

        file_contents = self._config_file_parser.serialize(config_items)
        for output_file_path in output_file_paths:
            with self._config_file_open_func(output_file_path, "w") as output_file:
                output_file.write(file_contents)

        if exit_after:
            self.exit(0)

    @staticmethod
    def get_arg_parser() -> configargparse.ArgumentParser:
        """Dynamically get the file path of the script that called the function, use the inspect module
        ref: chatgpt 4o"""
        logger_name: str = CustomArgumentParser.calling_directory()
        return get_argument_parser(logger_name)

    @staticmethod
    def calling_directory():
        # Get caller frame (1 step up the call stack)
        caller_frame: list = inspect.stack()[1]
        caller_file: str = caller_frame.filename
        logger_name: str = os.path.basename(os.path.dirname(caller_file))
        return logger_name

    def set_namespace_from_dict(self, args, config_dict: dict):
        """Set the namespace from a dictionary.

        Args:
            config_dict (dict): Dictionary containing configuration data.
        """
        # Convert dictionary to namespace

        # Update the namespace with command line arguments
        if config_dict:
            for key, value in config_dict.items():
                if hasattr(args, key):
                    setattr(args, key, value)
                    print(
                        f"Setting {key} to {value} from config file: Namespace: {args}")

        return args

    def set_namespace_from_config(self, args, config_file_path: str = default_path):
        # Load yaml file

        if os.path.exists(config_file_path):
            config_stream = self._config_file_open_func(
                config_file_path=config_file_path, k="r")
            config = self._config_file_parser.parse(config_stream)
            print(f"Config file loaded: {config}")
            if config:
                return self.set_namespace_from_dict(args, config)

        else:
            return args

    def namespace2dict(self, namespace: argparse.Namespace) -> dict:
        """Convert a namespace object to a dictionary.

        Args:
            namespace (argparse.Namespace): Namespace object to convert.

        Returns:
            dict: Dictionary representation of the namespace.
        """
        return vars(namespace)

    def parse_args(self):
        namespace = self.parse_known_args()[0]
        return namespace

    @staticmethod
    def read_from_yaml(config_file) -> dict:
        import yaml
        if config_file is not None:
            with open(config_file, "r") as file:
                yaml_args = yaml.safe_load(file)
            return yaml_args

    @staticmethod
    def read_from_json(config_file) -> dict:
        import json

        with open(config_file, "r") as f:
            json_args = json.load(f)
        return json_args

    @staticmethod
    def clean_config(self, config: dict):
        """
        Clean the configuration dictionary by:
        - Removing keys with None values.
        - Ensuring values are of type float, int, str, or None.
        - (Optional) Sorting keys alphabetically.

        Args:
            config (dict): Configuration dictionary to clean.

        Returns:
            dict: Cleaned configuration dictionary.
        """
        # Allowed value types
        allowed_types = (float, int, str, type(None))

        # Remove keys with invalid or None values
        cleaned_config = {
            k: v for k, v in config.items()
            if isinstance(v, allowed_types)
        }

        # (Optional) Sort the dictionary alphabetically
        # cleaned_config = dict(sorted(cleaned_config.items()))

        return cleaned_config


# global ArgumentParser instances
_parsers = {}


def init_argument_parser(name=None, **kwargs):
    """Creates a global ArgumentParser instance with the given name,
    passing any args other than "name" to the ArgumentParser constructor.
    This instance can then be retrieved using get_argument_parser(..)
    """

    if name is None:
        name = "default"

    if name in _parsers:
        raise ValueError(("kwargs besides 'name' can only be passed in the"
                          " first time. '%s' ArgumentParser already exists: %s") % (
            name, _parsers[name]))

    kwargs.setdefault('formatter_class',
                      argparse.ArgumentDefaultsHelpFormatter)
    kwargs.setdefault('conflict_handler', 'resolve')
    _parsers[name] = CustomArgumentParser(**kwargs)


def get_argument_parser(name=None, **kwargs):
    """Returns the global ArgumentParser instance with the given name. The 1st
    time this function is called, a new ArgumentParser instance will be created
    for the given name, and any args other than "name" will be passed on to the
    ArgumentParser constructor.
    """
    if name is None:
        name = "default"

    if len(kwargs) > 0 or name not in _parsers:
        init_argument_parser(name, **kwargs)

    return _parsers[name]
