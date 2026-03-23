"use strict";

const board = document.getElementById("board");
if (board) {
    const gameId = board.getAttribute("data-game-id");
    const playerId = parseInt(board.getAttribute("data-player-id"));
    const cells = document.querySelectorAll(".ttt-cell");
    const turnIndicator = document.getElementById("turnIndicator");
    const statusMessage = document.getElementById("statusMessage");
    const backToLobbyBtn = document.getElementById("backToLobbyBtn");

    const connection = new signalR.HubConnectionBuilder()
        .withUrl("/gameHub")
        .build();

    connection.on("GameStarted", function (p2Name, currentTurnPlayerId) {
        statusMessage.innerText = "Game in Progress";
        statusMessage.className = "text-success";
        document.getElementById("p2Name").innerText = p2Name;
        
        if (currentTurnPlayerId === playerId) {
            turnIndicator.innerHTML = `<span class="text-success fw-bold pulse-text">Your Turn!</span>`;
        } else {
            turnIndicator.innerHTML = `<span>Opponent's Turn</span>`;
        }
    });

    connection.on("ReceiveMove", function (position, marker, nextTurnPlayerId, status, winner) {
        const cell = cells[position];
        cell.innerText = marker;
        cell.classList.add(marker === "X" ? "cell-x" : "cell-o");

        if (status === "Finished") {
            board.classList.add("game-finished");
            backToLobbyBtn.style.display = "inline-block";
            statusMessage.innerText = "Game Over!";
            statusMessage.className = "text-info";
            
            if (winner === "Draw") {
                turnIndicator.innerHTML = `<span class="text-warning fw-bold fs-4 pulse-text">It's a Draw!</span>`;
            } else {
                turnIndicator.innerHTML = `<span class="text-info fw-bold fs-4 pulse-text">Winner: ${winner}</span>`;
            }
        } else {
            if (nextTurnPlayerId === playerId) {
                turnIndicator.innerHTML = `<span class="text-success fw-bold pulse-text">Your Turn!</span>`;
            } else {
                turnIndicator.innerHTML = `<span>Opponent's Turn</span>`;
            }
        }
    });

    connection.start().then(function () {
        connection.invoke("JoinGame", gameId).catch(function (err) {
            return console.error(err.toString());
        });
    }).catch(function (err) {
        return console.error(err.toString());
    });

    cells.forEach(cell => {
        cell.addEventListener("click", function (event) {
            const index = cell.getAttribute("data-index");
            if (cell.innerText === "" && !board.classList.contains("game-finished") && turnIndicator.innerText.includes("Your Turn")) {
                connection.invoke("MakeMove", gameId, playerId, parseInt(index)).catch(function (err) {
                    return console.error(err.toString());
                });
            }
        });
    });
}
