package schemas

type QuizParamsIn struct {
	Amount     int    `json:"amount" binding:"required,min=3"`
	Category   int    `json:"category" binding:"required"`
	Difficulty string `json:"difficulty" binding:"required,oneof=easy medium hard"`
}

type CreateSessionInput struct {
	PlayerID       int          `json:"player_id" binding:"required"` //mappano i campi del json alla struct
	PlayerUsername string       `json:"player_username" binding:"required"`
	MaxPlayers     int          `json:"max_players" binding:"required,gte=1"`
	Name           string       `json:"name" binding:"required"`
	Params         QuizParamsIn `json:"params" binding:"required"`
}

type JoinSessionInput struct {
	SessionID      string `json:"session_id" binding:"required"`
	PlayerID       int    `json:"player_id" binding:"required"`
	PlayerUsername string `json:"player_username" binding:"required"`
}

type LeaveSessionInput struct {
	SessionID string `json:"session_id" binding:"required"`
	PlayerID  int    `json:"player_id" binding:"required"`
}
