from io import TextIOWrapper
import os
import inspect
import argparse
import types
import glob
import sys
import configargparse
from collections import OrderedDict
import yaml
import json
from typing import Any

default_path = os.path.join(os.path.dirname(os.path.abspath(__file__)),
                            "python_config.yml")


def load_config_file(config_file_path: str, k: str = "r") -> dict[str, Any]:
    """Load a configuration file from the given path.

    Args:
        config_file_path (str): Path to the configuration file.

    Returns:
        dict: Parsed configuration data.
    """
    if not os.path.exists(config_file_path):

        if config_file_path.endswith('.yaml'):

            with open(config_file_path, k) as file:
                config_data = yaml.safe_load(file)

        elif config_file_path.endswith('.json'):
            with open(config_file_path, k) as file:
                config_data = json.load(file)
        else:
            raise ValueError(
                "Unsupported file format. Please use .yaml or .json files.")

        return config_data


class CustomConfigFileParser(configargparse.DefaultConfigFileParser):
    """
    Based on a simplified subset of INI and YAML formats. Here is the
    supported syntax

    .. code::

        # this is a comment
        ; this is also a comment (.ini style)
        ---            # lines that start with --- are ignored (yaml style)
        -------------------
        [section]      # .ini-style section names are treated as comments

        # how to specify a key-value pair (all of these are equivalent):
        name value     # key is case sensitive: "Name" isn't "name"
        name = value   # (.ini style)  (white space is ignored, so name = value same as name=value)
        name: value    # (yaml style)
        --name value   # (argparse style)

        # how to set a flag arg (eg. arg which has action="store_true")
        --name
        name
        name = True    # "True" and "true" are the same

        # how to specify a list arg (eg. arg which has action="append")
        fruit = [apple, orange, lemon]
        indexes = [1, 12, 35 , 40]

    """

    def get_syntax_description(self):
        msg = ("Config file syntax allows: key=value, flag=true, stuff=[a,b,c] "
               "(for details, see syntax at https://goo.gl/R74nmi).")
        return msg

    def parse(self, stream: TextIOWrapper | str | bytes) -> OrderedDict[str, Any]:
        """
        Parses the given input stream, string, or bytes as YAML and returns an OrderedDict of the contents.
        Args:
            stream (TextIOWrapper | str | bytes): The input to parse. Can be a file-like object, a string containing YAML, or bytes.
                - TextIOWrapper is a buffered text stream providing higher-level access to a BufferedIOBase buffered binary stream.
                  A buffered stream is a sequence of cached block bytes in RAM memory. 
        Returns:
            OrderedDict[str, Any]: An ordered dictionary containing the parsed YAML data.
        Raises:
            yaml.YAMLError: If the input cannot be parsed as valid YAML.
            TypeError: If the input type is not supported.
        """
       # see ConfigFileParser.parse docstring
        dict_loaded: dict[str, Any] = yaml.safe_load(
            stream)  # Load YAML content if needed

        # Turn dict to ordered dict
        items: OrderedDict[str, Any] = OrderedDict()

        for key, value in dict_loaded.items():
            items[key] = value

        return items

    def serialize(self, items: dict[str, Any]) -> str:
        # see ConfigFileParser.serialize docstring
        # What function to use to serialize the data
        r: str = yaml.dump(items)
        return r


class CustomArgumentParser(configargparse.ArgumentParser):

    """

    Custom argument parser that extends configargparse.ArgumentParser to handle configuration 
    files and command-line arguments.

    ref: https://github.com/bw2/ConfigArgParse

    """

    def __init__(self, ignore_unknown_config_file_keys: bool = True,
                 default_config_files: list[str] = [default_path],
                 **kwargs: Any) -> None:

        # TODO Confirm if unification of multiple instances to single config is feasible

        super().__init__(ignore_unknown_config_file_keys=ignore_unknown_config_file_keys,
                         default_config_files=default_config_files, config_file_parser_class=CustomConfigFileParser,
                         **kwargs)

        self.add_argument("-w", "--write-out-my-config", required=False, is_write_out_config_file_arg=True,
                          default=default_config_files[0],
                          help='write out config file path')

        self.variables_dict = {}

        self.arguments_key = "arguments"
        self.variables_key = "variables"

        # self._config_file_open_func = json.load
    def parse_known_args(
            self,
            args=None,
            namespace=None,
            config_file_contents=None,
            env_vars=os.environ,
            ignore_help_args=False,
    ):
        """Supports all the same args as the `argparse.ArgumentParser.parse_args()`,
        as well as the following additional args.

        Arguments:
            args: a list of args as in argparse, or a string (eg. "-x -y bla")
            config_file_contents (str). Used for testing.
            env_vars (dict). Used for testing.
            ignore_help_args (bool): This flag determines behavior when user specifies ``--help`` or ``-h``. If False,
                it will have the default behavior - printing help and exiting. If True, it won't do either.

        Returns:
            tuple[argparse.Namespace, list[str]]: tuple namescpace, unknown_args
        """
        if args is None:
            args = sys.argv[1:]
        elif isinstance(args, str):
            args = args.split()
        else:
            args = list(args)

        for a in self._actions:
            a.is_positional_arg = not a.option_strings

        if ignore_help_args:
            args = [arg for arg in args if arg not in ("-h", "--help")]

        # maps a string describing the source (eg. env var) to a settings dict
        # to keep track of where values came from (used by print_values()).
        # The settings dicts for env vars and config files will then map
        # the config key to an (argparse Action obj, string value) 2-tuple.
        self._source_to_settings = OrderedDict()
        if args:
            a_v_pair = (None, list(args))  # copy args list to isolate changes
            self._source_to_settings[configargparse._COMMAND_LINE_SOURCE_KEY] = {
                '': a_v_pair}

        # handle auto_env_var_prefix __init__ arg by setting a.env_var as needed
        if self._auto_env_var_prefix is not None:
            for a in self._actions:
                config_file_keys = self.get_possible_config_keys(a)
                if config_file_keys and not (a.env_var or a.is_positional_arg
                                             or a.is_config_file_arg or a.is_write_out_config_file_arg or
                                             isinstance(a, argparse._VersionAction) or
                                             isinstance(a, argparse._HelpAction)):
                    stripped_config_file_key = config_file_keys[0].strip(
                        self.prefix_chars)
                    a.env_var = (self._auto_env_var_prefix +
                                 stripped_config_file_key).replace('-', '_').upper()

        # add env var settings to the commandline that aren't there already
        env_var_args = []
        nargs = False
        actions_with_env_var_values = [a for a in self._actions
                                       if not a.is_positional_arg and a.env_var and a.env_var in env_vars
                                       and not configargparse.already_on_command_line(args, a.option_strings, self.prefix_chars)]
        for action in actions_with_env_var_values:
            key = action.env_var
            value = env_vars[key]
            # Make list-string into list.
            if action.nargs or isinstance(action, argparse._AppendAction):
                nargs = True
                if value.startswith("[") and value.endswith("]"):
                    # handle special case of k=[1,2,3] or other json-like syntax
                    try:
                        value = json.loads(value)
                    except Exception:
                        # for backward compatibility with legacy format (eg. where config value is [a, b, c] instead of proper json ["a", "b", "c"]
                        value = [elem.strip()
                                 for elem in value[1:-1].split(",")]
            env_var_args += self.convert_item_to_command_line_arg(
                action, key, value)

        if nargs:
            args = args + env_var_args
        else:
            args = env_var_args + args

        if env_var_args:
            self._source_to_settings[configargparse._ENV_VAR_SOURCE_KEY] = OrderedDict(
                [(a.env_var, (a, env_vars[a.env_var]))
                    for a in actions_with_env_var_values])

        # before parsing any config files, check if -h was specified.
        supports_help_arg = any(
            a for a in self._actions if isinstance(a, argparse._HelpAction))
        skip_config_file_parsing = supports_help_arg and (
            "-h" in args or "--help" in args)

        # prepare for reading config file(s)
        known_config_keys = {config_key: action for action in self._actions
                             for config_key in self.get_possible_config_keys(action)}

        # open the config file(s)
        config_streams = []
        if config_file_contents is not None:
            stream = configargparse.StringIO(config_file_contents)
            stream.name = "method arg"
            config_streams = [stream]
        elif not skip_config_file_parsing:
            config_streams = self._open_config_files(args)

        # parse each config file
        for stream in reversed(config_streams):
            try:
                config_items = self._config_file_parser.parse(stream)
            except configargparse.ConfigFileParserException as e:
                self.error(str(e))
            finally:
                if hasattr(stream, "close"):
                    stream.close()

            # add each config item to the commandline unless it's there already
            config_args = []
            nargs = False
            for key, value in config_items.items():
                if key in known_config_keys:
                    action = known_config_keys[key]
                    discard_this_key = configargparse.already_on_command_line(
                        args, action.option_strings, self.prefix_chars)
                else:
                    action = None
                    discard_this_key = self._ignore_unknown_config_file_keys or \
                        configargparse.already_on_command_line(
                            args,
                            [self.get_command_line_key_for_unknown_config_file_setting(
                                key)],
                            self.prefix_chars)

                if not discard_this_key:
                    config_args += self.convert_item_to_command_line_arg(
                        action, key, value)
                    source_key = "%s|%s" % (
                        configargparse._CONFIG_FILE_SOURCE_KEY, stream.name)
                    if source_key not in self._source_to_settings:
                        self._source_to_settings[source_key] = OrderedDict()
                    self._source_to_settings[source_key][key] = (action, value)
                    if (action and action.nargs or
                            isinstance(action, argparse._AppendAction)):
                        nargs = True

            if nargs:
                args = args + config_args
            else:
                args = config_args + args

        # save default settings for use by print_values()
        default_settings = OrderedDict()
        for action in self._actions:
            cares_about_default_value = (not action.is_positional_arg or
                                         action.nargs in [configargparse.OPTIONAL, configargparse.ZERO_OR_MORE])
            if (configargparse.already_on_command_line(args, action.option_strings, self.prefix_chars) or
                    not cares_about_default_value or
                    action.default is None or
                    action.default == configargparse.SUPPRESS or
                    isinstance(action, configargparse.ACTION_TYPES_THAT_DONT_NEED_A_VALUE)):
                continue
            else:
                if action.option_strings:
                    key = action.option_strings[-1]
                else:
                    key = action.dest
                default_settings[key] = (action, str(action.default))

        if default_settings:
            self._source_to_settings[configargparse._DEFAULTS_SOURCE_KEY] = default_settings

        # parse all args (including commandline, config file, and env var)
        namespace, unknown_args = argparse.ArgumentParser.parse_known_args(
            self, args=args, namespace=namespace)
        # handle any args that have is_write_out_config_file_arg set to true
        # check if the user specified this arg on the commandline
        self.save_config(namespace=namespace)
        return namespace, unknown_args

    def save_config(self, namespace: argparse.Namespace, configargparse_filter: bool = True):
        output_file_paths = [getattr(namespace, a.dest, None) for a in self._actions
                             if getattr(a, "is_write_out_config_file_arg", False)]
        output_file_paths = [a for a in output_file_paths if a is not None]
        self.write_config_file(namespace, output_file_paths,
                               exit_after=False, configargparse_filter=configargparse_filter)

    def _open_config_files(self, command_line_args: list[str]) -> list[Any]:
        """Tries to parse config file path(s) from within command_line_args.
        Returns a list of opened config files, including files specified on the
        commandline as well as any default_config_files specified in the
        constructor that are present on disk.

        Args:
            command_line_args: List of all args

        Returns:
            list[Any]: open config files
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

    def write_config_file(self, parsed_namespace, output_file_paths: list, exit_after: bool = False,
                          configargparse_filter: bool = True):
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
        if configargparse_filter:
            self.arguments_dict = self.get_items_for_config_file_output(
                self._source_to_settings, parsed_namespace)
        else:
            self.arguments_dict = self.namespace2dict(parsed_namespace)

        config_items = {self.arguments_key: self.ordereddict2dict(self.arguments_dict),
                        self.variables_key: self.variables_dict}

        file_contents: str = self._config_file_parser.serialize(config_items)
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

    def set_namespace_from_dict(self, args: argparse.Namespace, config_dict: dict[Any, Any] | OrderedDict[Any, Any]) -> argparse.Namespace:
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

    def set_namespace_from_config(self, args: argparse.Namespace, config_file_path: str = default_path) -> argparse.Namespace:
        # Load yaml file

        if os.path.exists(config_file_path):
            config_stream: TextIOWrapper = self._config_file_open_func(
                config_file_path, "r")

            config: OrderedDict[Any, Any] = self._config_file_parser.parse(
                config_stream)

            config: dict[Any, Any] = config[self.arguments_key]

            if config:
                args = self.set_namespace_from_dict(args, config)

        return args

    def namespace2dict(self, namespace: argparse.Namespace) -> dict[Any, Any]:
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
_parsers: dict[Any, configargparse.ArgumentParser] = {}


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


def get_argument_parser(name: Any = None, **kwargs) -> configargparse.ArgumentParser:
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
