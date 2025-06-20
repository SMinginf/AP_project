package models

type Player struct {
	ID       int    `json:"id"`
	Username string `json:"username"`
}

// QuizParams memorizza i parametri OpenTriviaDB
type QuizParams struct {
	Amount     int    `json:"amount"`
	Category   int    `json:"category"`
	Difficulty string `json:"difficulty"`
}

type Session struct {
	ID         string     `json:"id"`
	Name       string     `json:"name"`
	Params     QuizParams `json:"params"`
	Players    []Player   `json:"players"`
	MaxPlayers int        `json:"max_players"`
	IsOpen     bool       `json:"is_open"`
}
