"""
This script plots a data file.

Usage: python3 graph_it.py <data_file>
"""

import matplotlib.pyplot as plt
import sys


with open(sys.argv[1]) as f:
    values = [int(line) / 1e9 for line in f]

plt.axhline(y=0, color='k')
plt.plot(values)
plt.tight_layout()
plt.show()
