from __future__ import print_function
import numpy as np
import sys
import os
import argparse



#################################################################################
# Install Theano if it is not avalaible.                                        #
#################################################################################

def install(package_name):
    import importlib
    try:
        importlib.import_module(package_name)
    except ImportError:
        import pip
        pip.main(['install', '--user', package_name])
        import site
        if sys.version_info < (3, 0):
            reload(site)
        elif sys.version_info < (3, 4):
            import imp
            imp.reload(site)
        else:
            importlib.reload(site)


if True:
    install('theano')


#################################################################################
# Theano configs.                                                               #
# Please refer to http://deeplearning.net/software/theano/library/config.html . #
#################################################################################

import theano
#theano.config.device = 'gpu'
#String value: 'cpu', 'cuda', 'cuda0', 'cuda1', 'opencl0:0', 'opencl0:1', 'gpu', 'gpu0'

#theano.config.floatX = 'float32'
#String value: 'float64', 'float32', or 'float16' (with limited support)
#Default: 'float64'

#theano.config.mode = 'Mode'
#String value: 'Mode', 'DebugMode', 'FAST_RUN', 'FAST_COMPILE'

#theano.config.optimizer = 'None'
#String value: 'fast_run', 'merge', 'fast_compile', 'None'
#Default: 'fast_run'


#################################################################################
# Theano imports.                                                               #
#################################################################################

import theano.tensor as T
from theano import In
from theano import Out
from theano import function
from theano import shared
#from theano.tensor.shared_randomstreams import RandomStreams


###################################################################
# Variables                                                       #
# When launching project or scripts from Visual Studio,           #
# input_dir and output_dir are passed as arguments.               #
# Users could set them from the project setting page.             #
###################################################################

input_dir = None
output_dir = None
model_dir = None


def main():
    exit(0)


if __name__ == "__main__":
    parser = argparse.ArgumentParser()
    parser.add_argument("--input_dir", help="Input directory where where training dataset and meta data are saved", required=False)
    parser.add_argument("--output_dir", help="Output directory where output such as logs are saved.", required=False)

    args, unknown = parser.parse_known_args()
    input_dir = args.input_dir
    output_dir = args.output_dir
    model_dir = output_dir

    main()
