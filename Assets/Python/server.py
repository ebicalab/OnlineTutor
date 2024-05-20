from fastapi import FastAPI
from pydantic import BaseModel
import socket

app = FastAPI()

class Position(BaseModel):
    x: float
    y: float
    z: float

def send_to_unity(x, y, z):
    message = f'{{"x": {x}, "y": {y}, "z": {z}}}'
    client_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    client_socket.connect(('unity', 8052))
    client_socket.sendall(message.encode('utf-8'))
    client_socket.close()

@app.post("/update_position")
async def update_position(position: Position):
    send_to_unity(position.x, position.y, position.z)
    return {"status": "position updated"}
