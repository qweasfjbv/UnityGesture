import pandas as pd
import numpy as np
import matplotlib.pyplot as plt
from sklearn.metrics import confusion_matrix
import seaborn as sns
from sklearn.manifold import TSNE

# Load CSV
df = pd.read_csv('score_data.csv') 

# Reshape
data = df.to_numpy()                        # shape: (320, 17)
scores = data[:, :-1]                       # shape: (320, 16)
scores = scores.reshape(4, 80, 16)          # shape: (4, 80, 16)

spend_time = data[:, -1]                    # shape: (320,)
spend_time = spend_time.reshape(4, 80)      # shape: (4, 80)


def drawSpendTimeGraph():
    
    plt.title("Average Spend Time")
    plt.xlabel("Gesture")
    plt.ylabel("Average Spend Time")
    
    averaged = spend_time.reshape(4, 16, 5).mean(axis=2)

    labels = ["$1", "$P", "Protractor", "$P-RS"]
    colors = ['red', 'green', 'blue', 'purple']

    x = np.arange(1, 17)

    for i in range(4):
        plt.plot(x, averaged[i], label=labels[i], color=colors[i], marker='o')

    plt.legend()
    plt.grid(True)
    plt.tight_layout()
    plt.show()

def drawRecognizerAccuracy():
    correct_counts = np.zeros(4, dtype=int)

    plt.title("Correct Predictions per Recognizer")
    plt.xlabel("Recognizer")
    plt.ylabel("Correct Predictions (out of 80)")
    labels = ["$1", "$P", "Protractor", "$P-RS"]
    colors = ["red", "green", "blue", "purple"]

    for r in range(4):
        for i in range(16):
            for j in range(5):
                index = i * 5 + j
                predicted = np.argmax(scores[r, index])
                if predicted == i:
                    correct_counts[r] += 1

    plt.figure(figsize=(8, 6))
    plt.bar(labels, correct_counts, color=colors, width=0.5)

    plt.ylim(0, 80)
    plt.grid(axis='y')
    plt.tight_layout()
    plt.show()

def drawConfusionMatrix(idx):
    recognizer_idx = idx

    y_true = np.repeat(np.arange(16), 5)

    y_pred = np.argmax(scores[recognizer_idx], axis=1)
    
    cm = confusion_matrix(y_true, y_pred)
    
    plt.figure(figsize=(10, 8))
    sns.heatmap(cm, annot=True, fmt="d", cmap="Blues")
    plt.title("Confusion Matrix")
    plt.xlabel("Predicted")
    plt.ylabel("Actual")
    plt.show()

def drawTSNEVisualization(idx):
    features = scores[idx]
    labels = np.repeat(np.arange(16), 5)

    tsne = TSNE(n_components=2, perplexity=30, random_state=42)
    reduced = tsne.fit_transform(features)

    plt.figure(figsize=(10, 8))
    for i in range(16):
        idxs = labels == i
        plt.scatter(reduced[idxs, 0], reduced[idxs, 1], label=f"Gesture {i}", alpha=0.7)

    plt.title("TSNE")
    plt.legend(loc='upper right', bbox_to_anchor=(1.2, 1))
    plt.grid(True)
    plt.tight_layout()
    plt.show()

drawConfusionMatrix(3)
drawTSNEVisualization(3)