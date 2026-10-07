// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// Automatically trigger the AI turn after a short delay when the page is loaded.
const gameTable = document.querySelector(".game-table");
if (gameTable && gameTable.dataset.aiTurn === "true") {
    setTimeout(() => {
        fetch("/Game/AiTurn", {
            method: "POST"
        })
            .then(async response => {
                if (response.ok) {
                    const result = await response.json();
                    if (result.passed) {
                        playPassSound();
                    } else if (result.isLastCard) {
                        playLastCardSound();
                    } else {
                        playCardSound();
                    }
                }
            });
    }, 1000);
}

// Click event for each cards which shows selected cards.
document.querySelectorAll(".player-card").forEach(card => {
    card.addEventListener("click", function () {

        // Do nothing when it is not the player's turn.
        if (gameTable && gameTable.dataset.aiTurn === "true"){
        return;
        }
        this.classList.toggle("selected");
    });
});

// Import a card-playing sound
function playCardSound() {
    const cardSound = new Audio("/sound/card-play.mp3");
    cardSound.addEventListener("ended", function () {
        window.location.reload();
    });
    cardSound.play();
}

// Click event for the play button for playing the selected cards.
const playButton = document.getElementById("play-button");
playButton.addEventListener("click", function () {
    const selectedCards = document.querySelectorAll(".player-card.selected");
    const cards = Array.from(selectedCards).map(card => ({
        suit: card.dataset.suit,
        rank: card.dataset.rank
    }));
    if (selectedCards.length === 0) {
        showGameError("Please select at least one card to play."); 
        return;
    }
    fetch("/Game/PlayCards", {
        method: "POST",
        headers: {
            "Content-Type": "application/json"
        },
        body: JSON.stringify({
            cards: cards
        })
    })
        .then(async response => {
            if (response.ok) {
                const result = await response.json();
                if (result.isLastCard) {
                    playLastCardSound();
                } else {
                    playCardSound();
                }
                return;
            }
            const message = await response.text();
            showGameError(message);
        })
});

// Import a knock sound
function playPassSound() {
    const passSound = new Audio("/sound/table-knock.mp3");
    passSound.addEventListener("ended", function () {
        window.location.reload();
    });
    passSound.play();
}

// Click event for the pass button for passing the turn.
document.getElementById("pass-button").addEventListener("click", function () {
    fetch("/Game/Pass", {
        method: "POST"
    })
        .then(async response => {
            if (response.ok) {
                playPassSound();
                return;
            }
            const message = await response.text();  
            showGameError(message);
        });
});

// Import a "Last Card" Sound
// Card-Play Sound will also be executed
function playLastCardSound() {
    const cardSound = new Audio("/sound/card-play.mp3");
    const lastCardSound = new Audio("/sound/last-card.mp3");
    cardSound.play();
    setTimeout(function () {
        lastCardSound.play();
    }, 250);
    setTimeout(function () {
        window.location.reload();
    },1500);
}

// Show game error message if there is any.
let errorTimeout;
function showGameError(message) {
    const errorElement = document.getElementById("game-error");
    if (!errorElement) {
        return;
    }

    // Clear any existing timeout to prevent multiple messages from overlapping.
    clearTimeout(errorTimeout);

    // Set the error message and show the error element.
    errorElement.textContent = message;
    errorElement.classList.add("show");

    // Hide the error message after 2 seconds.
    errorTimeout = setTimeout(() => {
        errorElement.classList.remove("show");
    }, 2000);
}

// Click event for the New Game button.
const newGameButton = document.getElementById("new-game-button");

if (newGameButton) {
    newGameButton.addEventListener("click", function () {
        fetch("/Game/NewGame", {
            method: "POST"
        })
            .then(async response => {
                if (response.ok) {
                    window.location.reload();
                    return;
                }

                const message = await response.text();
                showGameError(message);
            });
    });
}