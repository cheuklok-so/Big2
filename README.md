# Big2
A web-based "Big Two" card game developed with C# and ASP.NET Core MVC

## Live Demo
https://big2app-e0bpfkdvf5ahbugr.southeastasia-01.azurewebsites.net/

## About this project
Big2 is a web-based implementation of the traditional Hong Kong "Big Two" card game, developed with C# and ASP.NET Core MVC
The project focuses on implementing the game rules, hand evaluation, turn management, and AI opponents with different playing strategies
The project was created for educational purposes. After working with the MVC pattern and ASP.NET for the first time at work, I wanted to build a small project independently to gain a better understanding of the framework

The main goals of this project are to:
・ Become more familiar with the ASP.NET Core MVC architecture
・ Practice C# and LINQ
・ Understand how Models, Views, and Controllers work together
・ Apply C# concepts to a complete application rather than isolated exercises
"Big Two" was chosen because its game rules provide opportunities to practice object-oriented design, collection manipulation, LINQ, and game logic

## Features
・ Four-Player Big Two game
・ Complete turn and round management
・ Pass system and automatic start of a new round after consecutive passes
・ Hand recognition and comparison for:
　・ Single
　・ Pair
　・ Triple
　・ Straight
　・ Flush
　・ Full House
　・ Four of a Kind
　・ Straight Flush
・ Big Two specific straight rules: A-2-3-4-5
・ Automatic card sorting
・ Web-based game interface

## Game Rules
This project uses the following "Big Two" rules:
・ The game is played by four players using a standard 52-card deck
・ The player holding the Three of Diamonds starts the game
・ The first play must contain the Three of Diamonds
・ Players must play a stronger hand of the same type and number of cards, or pass
・ After all other players pass, the last player who played a valid hand starts a new round
・ The first player to play all cards in their hand wins

### Card Ranking
Cards are ranked from lowest to highest:
3 < 4 < 5 < 6 < 7 < 8 < 9 < 10 < J < Q < K < A < 2 
Suits are ranked from lowest to highest:
Diamonds < Clubs < Hearts < Spades

### Five-Card Patterns
Five-card patterns are ranked from lowest to highest:
Straight < Flush < Full House < Four of a Kind < Straight Flush
For Straights:
・ 3-4-5-6-7 is the lowest straight
・ A-2-3-4-5 is the highest straight
・ 2-3-4-5-6 is not considered as a valid straight
・ J-Q-K-A-2 is not considered as a valid straight

## AI Players
Three AI opponents with different strategies
・ Conservative AI
・ Aggressive AI
・ Cunning AI

## Technologies
・ C#
・ ASP.NET Core MVC
・ LINQ
・ Razor Views
・ JavaScript
・  HTML / CSS

## Project Structure
The project follows the ASP.NET Core MVC Structure:
・ `Controllers/` - Handles user actions and game requests
・ `Models/Game/` - Contains the core game logic, including cards, players, hand evaluation, and game rules
・ `Models/AI/` - Contains AI players and their playing strategies
・ `Models/Session/` - Handles game session-related data
・ `Views/` - Contains Razor views for the user interface
・ `wwwroot/` - Contains static files such as CSS, JavaScript, Images and Sound Effects

## Getting Started

### Prerequisites
・ .NET 10 SDK

### Run
```bash
git clone <repository-url>
cd Big2
dotnet restore
dotnet run
```