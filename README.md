# Tic-Tac-Toe: Bot Challenge

This project features a Tic-Tac-Toe game board and 6 AI bots, each with a different difficulty level.

Instead of relying on predefined, sequential behaviors (like always starting in the center), these bots dynamically analyze the current state of the board to decide where to place their tokens. Building the logic for these decision-making processes was a highly interesting programming challenge.

## The Bots
There are 6 distinct bots, ranging from completely random to playing perfect:

Very Easy: Places tokens in random available spots on the board.

Easy: Attempts to build a straight line of tokens.

Medium: Attempts to build a straight line, but will block the opponent if they are about to win (two tokens in a row).

Hard-human: Simulates a highly skilled human player who occasionally makes mistakes.

Hard-Default: A standalone version of a specific algorithm (defaultTurn) used by the Expert bot. It acts as an upgraded version of the Medium bot.

Expert: Plays the perfect game. It is impossible to defeat.

## File Structure & Architecture


Game.cs: The main game board. The board is represented by a 3x3 nullable boolean matrix. True represents one player, False represents the other, and null represents an empty space. This class also manages turn order, who starts the game, and checks for win/draw conditions.

Form1.cs: This file is only included to provide a graphical user interface (GUI) and make it easier to play against the bots. It is not an important part of the core project logic and may contain errors.


## Bot Logic

### Bot.cs: 
The abstract base class for all bots. It contains a 3x3 boolean matrix called GameState which represents the board. Bots use this state to make their decisions. It includes an abstract function called playing, which is called to trigger the bot's turn.

### BotVeryEasy.cs: 
Inherits from Bot. It randomly selects an available space on the board. While the approach of picking a random spot and checking if it's empty is technically inefficient, the speed of C# and the small 3x3 board size make it practically instantaneous. An alternative would be scanning the matrix to store available spots first, but for this scale, random selection is just as fast or faster.

### BotEasy.cs: 
Inherits from Bot. This is a crucial file as it serves as the parent class for the more complex bots. It contains the logic to interpret the board and build straight lines. It first identifies "possible lines" (lines with no enemy tokens), checks which of those lines have the most friendly tokens, and randomly selects an available spot within those optimal lines.

### BotMedium.cs: 
Inherits from BotEasy. The logic is identical to BotEasy, except it also evaluates potential enemy lines. If the enemy has a line with 2 tokens and the bot does not have an immediate winning move, it blocks the enemy.

### botHard-human.cs: 
(Class: BotHard). Inherits from Bot. Internally, it contains instances of BotExpert, BotMedium, and BotEasy. It has a 50% chance to play as Expert, 40% as Medium, and 10% as Easy. This combination makes it better than the Medium robot; the idea is that it looks like a "human" who knows how to play but can make mistakes.

| Matchup | Bot 1 Win Rate | Bot 2 Win Rate |
| :--- | :--- | :--- |
| BotHard vs BotExpert | 0.00% | 38.10% |
| BotHard vs BotMedium | 36.20% | 20.20% |
| BotHard vs BotEasy | 84.00% | 9.70% |

### botHard-Default.cs: 
(Class: BotHardDefault). Inherits from BotExpert. This was built simply to isolate and test the defaultTurn function of the Expert bot. It plays surprisingly well, slightly outperforming the BotHard human.

| Matchup | Bot 1 Win Rate | Bot 2 Win Rate |
| :--- | :--- | :--- |
| BotHardDefault vs BotEasy | 83.00% | 8.70% |
| BotHardDefault vs BotMedium | 41.50% | 14.50% |
| BotHardDefault vs BotHard | 29.90% | 25.60% |
| BotHardDefault vs BotExpert | 0.00% | 38.00% |

### botExpert.cs 
Inherits from BotEasy. This was the most difficult bot to develop because the goal was perfect play; it had to be impossible to beat.
To achieve this, I first divided the early-game logic into two distinct states: Attack (when the bot plays first) and Defense (when the opponent plays first). I explicitly hardcoded the behavior for the first 2 turns as an attacker, and the 1st turn as a defender, because the early game lacks a mathematical pattern simple enough to automate effectively.

First Turn (Attack): The bot simply places a token on the board, prioritizing the corners as they represent the strongest positions.

Second Turn (Attack) & Board Normalization: This is where the logic gets interesting. The bot first "normalizes" the board. In Tic-Tac-Toe, the only thing that truly matters is the spatial relationship between the tokens (e.g., having one token in a corner and another in the center). It doesn't matter which specific corner it is. To drastically reduce the number of possible board states to evaluate, the bot treats any corner it played in as the top-left corner "0,0", and any edge as the top edge "0,1". After normalizing its own position, it evaluates the relative position of the opponent's token and decides where to place its second token based on that specific setup.

First Turn (Defense): This works very similarly to the second turn of an attack, but it's much simpler to calculate since there is only one enemy token on an otherwise empty board.

Dynamic Logic (defaultTurn):
From the 3rd turn in Attack, or the 2nd turn in Defense, the bot stops using hardcoded responses and begins executing the defaultTurn function. This function is an upgraded version of the Medium bot's logic. While the code isn't strictly sequential, the general priority list is:

Win: If there is a line with two friendly tokens, complete it.

Block: If the previous step didn't happen and the enemy has a line with two tokens, block them to prevent a loss.

Double Attack: If no one is about to win, try to execute a double attack (placing a token in a way that creates two separate winning spots for the next turn).

Smart Straight Line: If a double attack isn't possible, try to build a straight line while benefiting the enemy as little as possible (explained below).

Block Lines: If building a straight line isn't possible, try to block the enemy's straight lines.

Random: If the enemy has no straight lines, play randomly.

Minimizing Enemy Benefit:
When the bot places a token (assuming it already has one on the board) to create a threat, it forces the enemy to defend immediately or lose. The bot must execute its attack in a way that forces the enemy to play their defensive token in a spot that does not help the enemy build their own lines.

While it's sometimes impossible to completely avoid giving the enemy a somewhat useful spot, the goal is to minimize that benefit as much as possible.

Example: Imagine the bot has a token at 0,1 and the enemy has one at 1,0. If the bot places a token at 0,2 to build a straight line, it forces the enemy to defend at 0,0. By forcing the enemy into 0,0, we give them a dominant position and end up losing the game. The bot calculates to avoid these scenarios.

## Testing and Debugging

The files in the "Program Test" folder are scripts that I created with the help of Gemini AI specifically for testing the bots.

To use these scripts, you need to replace your main Program.cs file with one of these and change the file extension from .txt to .cs.

Program - BotVsBot:
With this script, you can match the bots against each other. You can view the win rate percentages and draws. Also, by setting the repasarVictorias configuration to True and using repasarBot to select the specific bot you want to review, you can watch some very interesting things unfold—for example, seeing the "Very Easy" bot execute a perfect game purely by random chance!

Program - TestBot:
With this script, you can play Ta-Te-Ti directly through the console, which makes debugging much easier.
