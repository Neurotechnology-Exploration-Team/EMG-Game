"""This file starts a TCP server and listens for incoming data."""
import sys
import socket

from constants import *


# create a TCP/IP socket
tcp_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)

# bind the socket to the port
tcp_socket.bind(('', LOCAL_TCP_PORT))

# listen for incoming connections
tcp_socket.listen(1)

def bytes_to_int(data):
    num = 0
    for byte in data:
        num *= 256
        num += byte
    if num & 0x00_80_00_00:
        # it's negative
        num = num - 0x01_00_00_00
    return num

while True:
    # wait for a connection
    print('Waiting for a connection', file=sys.stderr)
    connection, client_address = tcp_socket.accept()

    try:
        print('Connection from', client_address, file=sys.stderr)

        # receive the data in small chunks and print it
        while True:
            data = connection.recv(33)
            # print(data.decode(encoding='utf8'), end='', flush=True)
            # print('\n'*3)
            # print(repr(data))
            # print(len(data))
            # print()
            # print(data[2:5])
            value = bytes_to_int(data[2:5])
            print(value)
            if not data:
                break
    finally:
        # clean up the connection
        connection.close()
