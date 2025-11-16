DROP SCHEMA IF EXISTS deadpigeons CASCADE;
CREATE SCHEMA IF NOT EXISTS deadpigeons;

-- player table
CREATE TABLE deadpigeons.player (
    player_id     UUID PRIMARY KEY,
    first_name    TEXT NOT NULL,
    last_name     TEXT NOT NULL,
    email         TEXT NOT NULL UNIQUE,
    phone_number  TEXT NOT NULL,
    password_hash TEXT NOT NULL,
    is_active     BOOLEAN NOT NULL DEFAULT TRUE
);

-- admin table
CREATE TABLE deadpigeons.admin (
    admin_id      UUID PRIMARY KEY,
    first_name    TEXT NOT NULL,
    last_name     TEXT NOT NULL,
    email         TEXT NOT NULL UNIQUE,
    password_hash TEXT NOT NULL
);

-- game table 
CREATE TABLE deadpigeons.game (
    game_id          UUID PRIMARY KEY,
    winning_numbers  INTEGER[],
    draw_date        TIMESTAMPTZ,
    expiration_date  TIMESTAMPTZ NOT NULL,
    CHECK (winning_numbers IS NULL OR cardinality(winning_numbers) = 3)
);

-- transaction table
CREATE TABLE deadpigeons.transaction (
    transaction_id   UUID PRIMARY KEY,
    player_id        UUID NOT NULL,
    mobilepay_req_id TEXT NOT NULL UNIQUE,
    amount           NUMERIC(10,2) NOT NULL,
    status           TEXT NOT NULL,
    timestamp        TIMESTAMPTZ NOT NULL DEFAULT NOW(),

    CONSTRAINT fk_transaction_player
        FOREIGN KEY (player_id)
        REFERENCES deadpigeons.player(player_id)
        ON DELETE RESTRICT
);

-- board table
CREATE TABLE deadpigeons.board (
    board_id             UUID PRIMARY KEY,
    player_id            UUID NOT NULL,
    game_id              UUID NOT NULL,
    chosen_numbers       INTEGER[] NOT NULL,
    price                NUMERIC(10,2) NOT NULL,
    is_repeating         BOOLEAN NOT NULL DEFAULT FALSE,
    repeat_until_game_id UUID,
    timestamp            TIMESTAMPTZ NOT NULL DEFAULT NOW(),

    CHECK (cardinality(chosen_numbers) BETWEEN 5 AND 8),

    CONSTRAINT fk_board_player
        FOREIGN KEY (player_id)
        REFERENCES deadpigeons.player(player_id)
        ON DELETE RESTRICT,

    CONSTRAINT fk_board_game
        FOREIGN KEY (game_id)
        REFERENCES deadpigeons.game(game_id)
        ON DELETE RESTRICT,

    CONSTRAINT fk_board_repeat_game
        FOREIGN KEY (repeat_until_game_id)
        REFERENCES deadpigeons.game(game_id)
        ON DELETE RESTRICT
);

-- winning board table
CREATE TABLE deadpigeons.winningboard (
    winningboard_id         UUID PRIMARY KEY,
    game_id                 UUID NOT NULL,
    board_id                UUID NOT NULL,
    winning_numbers_matched INTEGER NOT NULL,
    timestamp               TIMESTAMPTZ NOT NULL DEFAULT NOW(),

    CONSTRAINT fk_wb_game
        FOREIGN KEY (game_id)
        REFERENCES deadpigeons.game(game_id)
        ON DELETE RESTRICT,

    CONSTRAINT fk_wb_board
        FOREIGN KEY (board_id)
        REFERENCES deadpigeons.board(board_id)
        ON DELETE RESTRICT
);
