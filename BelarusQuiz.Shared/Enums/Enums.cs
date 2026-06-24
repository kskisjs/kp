namespace BelarusQuiz.Shared.Enums;

public enum GameMode { Battle, Solo }
public enum RoomStatus { Waiting, Playing, Finished }
public enum QuestionType { MultipleChoice, TrueFalse }

// НОВОЕ: категории вопросов
public enum QuizCategory
{
    All,        // Все категории (случайные вопросы из всех)
    Geography,  // 🗺️ География
    History,    // 📜 История
    Culture,    // 🎭 Культура
    Nature,     // 🌿 Природа
    Symbols,    // 🏛️ Символика
    People      // 🧑 Знаменитые люди
}