import json
import numpy as np
import tensorflow as tf
import tf2onnx
import os

DATA_PATH = r"C:\waheed\code\AdaptiveAITrainer\data\gameplay_data.json"
ONNX_PATH = r"C:\waheed\code\AdaptiveFightingAI\Assets\AI\enemy_ai.onnx"

NUM_FEATURES = 13
NUM_ACTIONS = 9


def smart_action_label(frame):
    action = frame["action"]
    distance = frame["distance"]

    pvx = frame["playerVelocity"]["x"]
    pvz = frame["playerVelocity"]["z"]

    player_hp = frame["playerHealth"]
    enemy_hp = frame["enemyHealth"]

    if action == "Punch":
        return 2

    if action == "Kick":
        return 3

    if action == "Block":
        return 4

    if enemy_hp < 30:
        return 5  # Retreat

    if player_hp < 30:
        return 8  # AggressivePush

    if action == "Move":
        if abs(pvx) > abs(pvz):
            if pvx > 0:
                return 7  # StrafeRight
            else:
                return 6  # StrafeLeft

        if distance > 3:
            return 1  # Advance

    return 0  # Idle


def load_data():
    if not os.path.exists(DATA_PATH):
        return None, None

    with open(DATA_PATH, "r") as f:
        data = json.load(f)

    if "frames" not in data or len(data["frames"]) == 0:
        return None, None

    X = []
    y = []

    for frame in data["frames"]:
        px = frame["playerPosition"]["x"]
        pz = frame["playerPosition"]["z"]

        ex = frame["enemyPosition"]["x"]
        ez = frame["enemyPosition"]["z"]

        pvx = frame["playerVelocity"]["x"]
        pvz = frame["playerVelocity"]["z"]

        evx = frame["enemyVelocity"]["x"]
        evz = frame["enemyVelocity"]["z"]

        distance = frame["distance"]

        player_hp = frame["playerHealth"]
        enemy_hp = frame["enemyHealth"]

        player_block = 1.0 if frame["playerBlocking"] else 0.0
        enemy_block = 1.0 if frame["enemyBlocking"] else 0.0

        features = [
            px,
            pz,
            ex,
            ez,
            distance,
            player_hp,
            enemy_hp,
            pvx,
            pvz,
            evx,
            evz,
            player_block,
            enemy_block
        ]

        X.append(features)
        y.append(smart_action_label(frame))

    return (
        np.array(X, dtype=np.float32),
        np.array(y, dtype=np.int32)
    )


def build_model():
    model = tf.keras.Sequential([
        tf.keras.Input(shape=(NUM_FEATURES,)),
        tf.keras.layers.Dense(256, activation="relu"),
        tf.keras.layers.Dense(128, activation="relu"),
        tf.keras.layers.Dense(64, activation="relu"),
        tf.keras.layers.Dense(NUM_ACTIONS, activation="softmax")
    ])

    model.compile(
        optimizer=tf.keras.optimizers.Adam(
            learning_rate=0.003
        ),
        loss="sparse_categorical_crossentropy",
        metrics=["accuracy"]
    )

    return model


def export_onnx(model):
    spec = (
        tf.TensorSpec(
            (None, NUM_FEATURES),
            tf.float32,
            name="input"
        ),
    )

    model.output_names = ["output"]

    tf2onnx.convert.from_keras(
        model,
        input_signature=spec,
        opset=13,
        output_path=ONNX_PATH
    )


def main():
    unity_ai_folder = os.path.dirname(ONNX_PATH)

    if not os.path.exists(unity_ai_folder):
        os.makedirs(unity_ai_folder)

    model = build_model()

    X, y = load_data()

    if X is not None and len(X) > 0:
        model.fit(
            X,
            y,
            epochs=50,
            batch_size=32,
            verbose=1
        )
    else:
        print("No gameplay data found.")

    export_onnx(model)

    print("FAST adaptive model exported.")
    print(ONNX_PATH)


if __name__ == "__main__":
    main()  