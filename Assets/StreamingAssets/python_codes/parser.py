import configargparse
import os
import inspect
from . import logger
import argparse
import types
import glob



class CustomArgumentParser(configargparse.ArgumentParser):

    """

    Custom argument parser that extends configargparse.ArgumentParser to handle configuration 
    files and command-line arguments.

    ref: https://github.com/bw2/ConfigArgParse

    """

    def __init__(self, ignore_unknown_config_file_keys=True,
                 default_config_files=[os.path.join(os.getcwd(), "python_config.json")],
                 **kwargs):
        
        #TODO Confirm if unification of multiple instances to single config is feasible

        super().__init__(ignore_unknown_config_file_keys=ignore_unknown_config_file_keys,
                 default_config_files=default_config_files,
                 **kwargs)
        
        self.add_argument('-c', '--my-config', required=False, is_config_file=True, help='config file path')
        self.add_argument("-w", "--write-out-my-config", required=True, default=os.path.join(os.getcwd(), "python_config.json"), 
                 help='write out config file path')
        
        self.variables_dict = {}

        self.arguments_key = "arguments"
        self.variables_key = "variables"

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
                        config_files.append(self._config_file_open_func(f)[self.arguments_key])

            except Exception as e:
                logger.error("Unable to open default config file(s): %s. Error: %s")

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

    def write_config_file(self, parsed_namespace, output_file_paths, exit_after=False):
        """Write the given settings to output files.

        Args:
            parsed_namespace: namespace object created within parse_known_args()
            output_file_paths: any number of file paths to write the config to
            exit_after: whether to exit the program after writing the config files
        """
        for output_file_path in output_file_paths:
            # validate the output file path
            try:
                with self._config_file_open_func(output_file_path, "w") as output_file:
                    pass
            except IOError as e:
                raise ValueError("Couldn't open {} for writing: {}".format(
                    output_file_path, e))
        if output_file_paths:
            # generate the config file contents
            self.arguments_dict = self.get_items_for_config_file_output(
                self._source_to_settings, parsed_namespace)
            
            config_items = {self.arguments_key:self.arguments_dict, self.variables_key:self.variables_dict}

            file_contents = self._config_file_parser.serialize(config_items)
            for output_file_path in output_file_paths:
                with self._config_file_open_func(output_file_path, "w") as output_file:
                    output_file.write(file_contents)

            print("Wrote config file to " + ", ".join(output_file_paths))
            if exit_after:
                self.exit(0)

    def get_arg_parser(self) -> configargparse.ArgumentParser:
        """Dynamically get the file path of the script that called the function, use the inspect module
        ref: chatgpt 4o"""
        logger_name : str = self.calling_directory()
        return self.super().get_argument_parser(logger_name)
    
    @staticmethod
    def calling_directory():
        # Get caller frame (1 step up the call stack)
        caller_frame : list = inspect.stack()[1]
        caller_file : str = caller_frame.filename
        logger_name : str = os.path.basename(os.path.dirname(caller_file))
        return logger_name
    
    def parse_args(self):
        return self.super().parse_known_args()

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
