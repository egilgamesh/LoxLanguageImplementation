﻿namespace CraftInterpreter.Lox;

public enum TokenType
{
	Undefined,
	LeftParenthesis,
	RightParenthesis,
	LeftBrace,
	RightBrace,
	LeftBracket,
	RightBracket,
	Bang,
	BangEqual,
	Equal,
	EqualEqual,
	Greater,
	GreaterEqual,
	Less,
	LessEqual,
	PlusPlus,
	MinusMinus,
	Question,
	Colon,
	Modulus,

	// Literals
	Identifier,
	String,
	Number,
	Comma,
	Dot,
	Minus,
	Plus,
	Semicolon,
	Star,
	Slash,
	And,
	Class,
	Else,
	False,
	Fun,
	For,
	If,
	Nil,
	Or,
	Print,
	Return,
	SuperClass,
	This,
	True,
	Var,
	While,
	Break,
	Eof
}