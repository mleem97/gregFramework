import os
import json
import time
import sys

RALPH_PATH = ".ralph"
TASKS_PATH = f"{RALPH_PATH}/tasks"
STATE_FILE = f"{RALPH_PATH}/state.json"

def init_env():
    """Initializes the Ralph workflow environment and directories."""
    os.makedirs(TASKS_PATH, exist_ok=True)
    if not os.path.exists(STATE_FILE):
        with open(STATE_FILE, "w") as f:
            json.dump({
                "session_start": time.time(), 
                "total_tasks_created": 0, 
                "status": "idle"
            }, f, indent=4)
        print("Initialized new Ralph environment.")

def print_status():
    """Calculates and prints the current status of all asynchronous tasks."""
    if not os.path.exists(TASKS_PATH):
        print("Error: Ralph system is not initialized. Run initialization first.")
        return
    
    todos = len([f for f in os.listdir(TASKS_PATH) if f.endswith(".todo")])
    locks = len([f for f in os.listdir(TASKS_PATH) if f.endswith(".lock")])
    dones = len([f for f in os.listdir(TASKS_PATH) if f.endswith(".done")])
    
    print("\n" + "="*30)
    print("=== RALPH DISPATCHER STATUS ===")
    print("="*30)
    print(f" ⏳ PENDING (TODO) : {todos}")
    print(f" 🔒 IN PROGRESS    : {locks}")
    print(f" ✅ COMPLETED      : {dones}")
    print("="*30 + "\n")

if __name__ == "__main__":
    init_env()
    if len(sys.argv) > 1 and sys.argv[1] == "status":
        print_status()
    else:
        print("Ralph Dispatcher is ready. Use the Orchestrator AI to plan and atomize tasks.")
