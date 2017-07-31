from __future__ import print_function
import cntk
import numpy as np
import scipy.sparse

# Define the task.
input_dim = 2    # classify 2-dimensional data
num_classes = 2  # into one of two classes


np.random.seed(0)
def generate_synthetic_data(N):
    Y = np.random.randint(size=N, low=0, high=num_classes)  # labels
    print(Y)
    X = (np.random.randn(N, input_dim)+3) * (Y[:,None]+1)   # data
    print(X)
    # Our model expects float32 features, and cross-entropy expects one-hot encoded labels.
    Y = scipy.sparse.csr_matrix((np.ones(N,np.float32), (range(N), Y)), shape=(N, num_classes))
    print(Y)
    X = X.astype(np.float32)
    print(X)
    return X, Y

X_train, Y_train = generate_synthetic_data(20)
# X_test,  Y_test  = generate_synthetic_data(1024)