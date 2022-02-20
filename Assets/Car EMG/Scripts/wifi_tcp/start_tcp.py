import requests
from constants import *


"""
You can make a POST request to the board to start the TCP server.

route: /tcp
body: {
  "delimiter": false,
  "ip": "string",
  "latency": 10000,
  "output": "raw",
  "port": 0,
  "sample_numbers": false,
  "timestamps": true
}
"""

print('Sending GET request to get board info...')
res = requests.get(f"http://{BOARD_IP}/all")
print('Got response:', res.status_code)
print(repr(res.text))
print()

#"""
# Start the TCP server
print('Sending POST request to start TCP server...')
res = requests.post(f"http://{BOARD_IP}/tcp", json={
    "delimiter": False,
    "ip": "192.168.4.2",
    "latency": 10000,
    "output": "raw",
    "port": LOCAL_TCP_PORT,
    "sample_numbers": False,
    "timestamps": True,
})
print('Got response:', res.status_code)
print(repr(res.text))
print()

input('Press enter to start stream')

print('Sending GET request to start stream...')
res = requests.get(f"http://{BOARD_IP}/output/json")
print('Got response:', res.status_code)
print(repr(res.text))
print()

res = requests.get(f"http://{BOARD_IP}/stream/start")
print('Got response:', res.status_code)
print(repr(res.text))
print()

input('Press enter to stop stream')

print('Sending GET request to stop stream...')
res = requests.get(f"http://{BOARD_IP}/stream/stop")
print('Got response:', res.status_code)
print(repr(res.text))
print()

"""



print('Sending GET request to update board...')
res = requests.get(f"http://{BOARD_IP}/update")
print('Got response:', res.status_code)
print(repr(res.text))
print()
#"""