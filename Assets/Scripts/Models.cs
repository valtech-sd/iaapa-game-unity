using Newtonsoft.Json;
using System.Collections.Generic;
using JetBrains.Annotations;

public enum MessageType {
	GameState,
	Leaderboard,
	TurnStart,
	Unknown
}

public enum GameState {
	Idle,
	Load,
	Run,
	End,
	Unknown
}

public enum PartyState {
	Idle,
	Qualifiers,
	PreTournament,
	Tournament,
	PostTournament,
	FreePlay,
	Unknown
}

public class SerializableAsJson {
	public override string ToString() {
		return JsonConvert.SerializeObject(this, Formatting.Indented);
	}
}

public class GameFlags: SerializableAsJson {
	[JsonProperty("leaderboardEnabled")]
	public bool LeaderboardEnabled { get; set; }
}

public class GameStateData: SerializableAsJson  {
	[JsonProperty("gameId")]
	[CanBeNull]
	public string GameId { get; set; }

	[JsonProperty("gameStatus")]
	[CanBeNull]
	public string GameStatus { get; set; }

	[JsonProperty("gameStartTimestamp")]
	[CanBeNull]
	public long? GameStartTimestamp { get; set; } //in milliseconds

	[JsonProperty("gameEndTimestamp")]
	[CanBeNull]
	public long? GameEndTimestamp { get; set; } //in milliseconds

	[JsonProperty("gameLengthMs")]
	[CanBeNull]
	public long? GameLength { get; set; } //in milliseconds

	[JsonProperty("flags")]
	[CanBeNull]
	public GameFlags Flags { get; set; }

	[JsonProperty("locations")]
	[CanBeNull]
	public List<Seat> Locations { get; set; }
}

public class Seat: SerializableAsJson  {
	[JsonProperty("location")]
	public int Location { get; set; }

	[JsonProperty("score")]
	public int Score { get; set; }

	[JsonProperty("playerName")]
	public string PlayerName { get; set; }

	[JsonProperty("playerId")]
	public string PlayerId { get; set; }
}

public class GameStateMessage: SerializableAsJson  {
	[JsonProperty("type")]
	public string Type { get; set; }

	[JsonProperty("timestamp")]
	public long Timestamp { get; set; } //in milliseconds

	[JsonProperty("data")]
	public GameStateData Data { get; set; }
}


public class LeaderboardData: SerializableAsJson  {
	[JsonProperty("leaderboardType")]
	public string LeaderboardType { get; set; }

	[JsonProperty("leaderboard")]
	public List<LeaderboardEntry> Leaderboard { get; set; }
}

public class LeaderboardEntry: SerializableAsJson  {
	[JsonProperty("rank")]
	public int Rank { get; set; }

	[JsonProperty("score")]
	public int Score { get; set; }

	[JsonProperty("playerName")]
	public string PlayerName { get; set; }

	[JsonProperty("playerId")]
	public string PlayerId { get; set; }
}

public class LeaderboardMessage: SerializableAsJson  {
	[JsonProperty("type")]
	public string Type { get; set; }

	[JsonProperty("timestamp")]
	public long Timestamp { get; set; } //in milliseconds

	[JsonProperty("data")]
	public LeaderboardData Data { get; set; }
}

public class TurnStartData: SerializableAsJson  {
	[JsonProperty("gameId")]
	public string GameId { get; set; }

	[JsonProperty("playerId")]
	public string PlayerId { get; set; }

	[JsonProperty("turnNumber")]
	public int TurnNumber { get; set; }

	[JsonProperty("turnLengthMs")]
	public int TurnLengthMs { get; set; }
}

public class TurnStartMessage: SerializableAsJson  {
	[JsonProperty("type")]
	public string Type { get; set; }

	[JsonProperty("timestamp")]
	public long Timestamp { get; set; }

	[JsonProperty("data")]
	public TurnStartData Data { get; set; }
}

public class MessageBrokerSettings: SerializableAsJson  {
	[JsonProperty("host")]
	public string Host { get; set; }
	[JsonProperty("user")]
	public string User { get; set; }
	[JsonProperty("pass")]
	public string Pass { get; set; }
	[JsonProperty("exchange")]
	public string Exchange { get; set; }
}

public class Config: SerializableAsJson  {
	[JsonProperty("messageBroker")]
	public MessageBrokerSettings MessageBroker { get; set; }
}
