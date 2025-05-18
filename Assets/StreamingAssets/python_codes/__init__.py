from python_logger import get_logger
import parser

if __name__ == "__main__":
    logger = get_logger()
    arg_parser = parser.CustomArgumentParser().get_arg_parser()

    # Write config file. I fear that printing to stout will mess with logging of 
    #the python thread as we have logs printed out in the standard output file.


    


