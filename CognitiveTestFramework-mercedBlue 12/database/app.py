from flask import Flask, request, jsonify
import mariadb
import sys
import secrets
from werkzeug.security import generate_password_hash, check_password_hash
app = Flask(__name__)

# Connect to MariaDB
try:
    conn = mariadb.connect(
        user="heidiaccess",
        password="Semheidi25*",  
        host="localhost",
        port=3306, 
        database="unity_database" 
    )
    cursor = conn.cursor()
except mariadb.Error as e:
    print(f"Error connecting to MariaDB: {e}") 
    sys.exit(1) 
print("Flask app loaded successfully!")
#==================================LOGIN===============================================
@app.route("/auth/login", methods=["POST"])
def login():
    player_name = request.form.get("player_name")
    password = request.form.get("password")
    if not player_name or not password:
        return jsonify({"error": "Missing form data"}), 400
    try:
        player_name = str(player_name)
        cursor.execute("SELECT hashed_password FROM users WHERE player_name = %s", (player_name,))
        result = cursor.fetchone()
        if not result:
            return jsonify({"Error": "User not found"}), 404
        stored_password = result[0]
        if check_password_hash(stored_password, password): 
            token = secrets.token_hex(32)
            cursor.execute("INSERT INTO session (token) VALUES (?)", (token,))
            session_id = cursor.lastrowid            
            conn.commit()
            return jsonify({"token": token, "session_id": session_id}), 200
        else:
            return jsonify({"Error": "Incorrect password"}), 401
    except Exception as e:
        return jsonify({"error": str(e)}), 500  
#==================================CREATE SESSION===============================================
@app.route("/session", methods=["POST"])
def session():
    player_name = request.form.get("player_name")
    token = request.form.get("token")
    if not player_name:
        return jsonify({"error": "Missing form data"}), 400
    
    try:
        cursor.execute("SELECT user_id FROM users WHERE player_name = %s", (player_name,))
        result = cursor.fetchone()
        if result is None:
            return jsonify({"error": "User not found"}), 404
        stored_user_id = result[0]

        cursor.execute("UPDATE session SET user_session_id = %s WHERE token = %s", (stored_user_id, token))
        conn.commit()
        print("session_id: ", stored_user_id)

        return jsonify({"session_id": stored_user_id}) #return the session id to store in unity until entire game finish
    
    except mariadb.Error as e:
        print("Error saving session")
        return jsonify({"Error": str(e)}), 500
#==================================GET USER DETAILS===============================================
@app.route("/scores", methods=["GET"]) 
def scores(): 
    token = request.headers.get("Authorization").replace("Bearer","").strip()
    print(token)
    cursor.execute("SELECT user_id FROM users where player_name = %s", (token,))
    result = cursor.fetchone()
    stored_user_id = result[0]

    cursor.execute("SELECT total_score, date_created, max_score, test_duration FROM session WHERE user_session_id = %s", (stored_user_id,)) 
    result = cursor.fetchall()
    columns = [desc[0] for desc in cursor.description]
    data = [dict(zip(columns, row)) for row in result]
    print(data)
    return jsonify(data), 200

#==================================POST SCORE TO EXISTING USER===================================
@app.route("/session/games", methods=["POST"])
def post_score():
    sessionID = request.form.get("sessionID")
    gameType = request.form.get("gameType")
    score = request.form.get("score")

    if not sessionID or not gameType or score is None:   
        return jsonify({"error": "Missing form data"}), 400
    
    try:
        score = int(score)
        cursor.execute("INSERT INTO game_result (game_session_id, game_type, score) VALUES (?, ?, ?)", (sessionID, gameType, score))
        conn.commit()
        print("Game data added")
        return jsonify({"message": "Score added!"}), 201
    
    except mariadb.Error as e:
        print("ERROR")
        return jsonify({"error": str(e)}), 500
#==================================Save the total game score of current user session===================================
@app.route("/session/total_score", methods=["POST"])
def total_score():
    token = request.form.get("token")
    total_score = request.form.get("total_score")
    max_score = request.form.get("max_score")
    test_duration = request.form.get("test_duration")
    try:
        cursor.execute("UPDATE session SET total_score = %s, max_score = %s, test_duration = %s WHERE token = %s", (total_score, max_score, test_duration, token))   
        conn.commit()
        return jsonify({"Status": "success"}), 200
    except:
        print("Error saving total score")
        return jsonify({"error": str(e)}), 500
#==================================POST NEW USER===============================================
@app.route("/auth/register", methods=["POST"]) #create new user
def register():
    player_name = request.form.get("player_name")
    password = request.form.get("password")
    hashedPassword = generate_password_hash(password)
    if not player_name or not password:
        return jsonify({"error": "Missing form data"}), 400
    try:
        player_name = str(player_name)
        hashedPassword = str(hashedPassword)
        cursor.execute(
            "INSERT INTO users (player_name, hashed_password) VALUES (?, ?)",
            (player_name, hashedPassword)
        ) 
        conn.commit()
        print("player registered")

        return jsonify({"message": "Player added!"}), 201
    except mariadb.Error as e:
        print("ERROR")
        return jsonify({"error": str(e)}), 400
    
if __name__ == "__main__":
    app.run(host="0.0.0.0", port=5000)
    #app.run(debug=True)
