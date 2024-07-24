using System.Collections.Generic;

namespace CupheadArchipelago.AP {
    public abstract class APObject {
        public string Name { get; private set; }
        public long Id { get; private set; }

        public APObject(string name, long id) {
            Name = name;
            Id = id;
        }

        public static implicit operator long(APObject a) => a.Id;
        public override string ToString() { return Name; }
    }

    public class APItem : APObject {
        private static readonly Dictionary<long,string> id_to_name = new();
        private static readonly Dictionary<string,long> name_to_id = new();
        public APItem(string name, long id) : base(name,id) { 
            id_to_name.Add(id,name);
            name_to_id.Add(name,id);
        }

        public static readonly APItem ability_dash = new APItem("Dash", 0xc4eae7);
        public static readonly APItem ability_duck = new APItem("Duck", 0xc4eae6);
        public static readonly APItem ability_parry = new APItem("Parry", 0xc4eae8);
        public static readonly APItem charm_coffee = new APItem("Coffee", 0xc4eae0);
        public static readonly APItem charm_dlc_broken_relic = new APItem("Broken Relic", 0xc4fad6);
        public static readonly APItem charm_dlc_cookie = new APItem("Astral Cookie", 0xc4fad4);
        public static readonly APItem charm_dlc_heartring = new APItem("Heart Ring", 0xc4fad5);
        public static readonly APItem charm_heart = new APItem("Heart", 0xc4eadd);
        public static readonly APItem charm_psugar = new APItem("P. Sugar", 0xc4eadf);
        public static readonly APItem charm_smokebomb = new APItem("Smoke Bomb", 0xc4eade);
        public static readonly APItem charm_twinheart = new APItem("Twin Heart", 0xc4eae1);
        public static readonly APItem charm_whetstone = new APItem("Whetstone", 0xc4eae2);
        public static readonly APItem coin = new APItem("Coin", 0xc4ead2);
        public static readonly APItem coin2 = new APItem("2 Coins", 0xc4ead3);
        public static readonly APItem coin3 = new APItem("3 Coins", 0xc4ead4);
        public static readonly APItem contract = new APItem("Contract", 0xc4ead6);
        public static readonly APItem dlc_ingredient = new APItem("Ingredient", 0xc4fad0);
        public static readonly APItem extrahealth = new APItem("+1 Health", 0xc4ead0);
        public static readonly APItem goal_devilko = new APItem("The Devil's Surrender", 0xc4eaf8);
        public static readonly APItem goal_dlc_saltbakerko = new APItem("Chef Saltbaker's Surrender", 0xc4fada);
        public static readonly APItem progressive_plane = new APItem("Progressive Plane", 0xc4ead5);
        public static readonly APItem super_i = new APItem("Super Art I", 0xc4eae3);
        public static readonly APItem super_ii = new APItem("Super Art II", 0xc4eae4);
        public static readonly APItem super_iii = new APItem("Super Art III", 0xc4eae5);
        public static readonly APItem superrecharge = new APItem("Super Recharge", 0xc4ead1);
        public static readonly APItem trap_fingerjam = new APItem("Finger Jam Trap", 0xc4eaf1);
        public static readonly APItem trap_inktrap = new APItem("Ink Trap", 0xc4eaf4);
        public static readonly APItem trap_slowfire = new APItem("Slow Fingers Trap", 0xc4eaf2);
        public static readonly APItem trap_superdrain = new APItem("Super Meter Drain Trap", 0xc4eaf3);
        public static readonly APItem weapon_charge = new APItem("Charge", 0xc4eadb);
        public static readonly APItem weapon_chaser = new APItem("Chaser", 0xc4ead9);
        public static readonly APItem weapon_dlc_converge = new APItem("Converge", 0xc4fad2);
        public static readonly APItem weapon_dlc_crackshot = new APItem("Crackshot", 0xc4fad1);
        public static readonly APItem weapon_dlc_twistup = new APItem("Twist-Up", 0xc4fad3);
        public static readonly APItem weapon_lobber = new APItem("Lobber", 0xc4eada);
        public static readonly APItem weapon_peashooter = new APItem("Peashooter", 0xc4ead7);
        public static readonly APItem weapon_roundabout = new APItem("Roundabout", 0xc4eadc);
        public static readonly APItem weapon_spread = new APItem("Spread", 0xc4ead8);

        public static APItem FromId(long id) => new APItem(id_to_name[id], id);
        public static bool IdExists(long id) => id_to_name.ContainsKey(id);
        public static bool NameExists(string name) => id_to_name.ContainsValue(name);
        public static string IdToName(long id) => id_to_name[id];
        public static long NameToId(string name) => name_to_id[name];
    }

    public class APLocation : APObject {
        private static readonly Dictionary<long,string> id_to_name = new();
        private static readonly Dictionary<string,long> name_to_id = new();
        public APLocation(string name, long id) : base(name,id) { 
            id_to_name.Add(id,name);
            name_to_id.Add(name,id);
        }

        public static readonly APLocation coin_isle1_secret = new APLocation("Inkwell Isle One Secret Coin", 0xc4eb42);
        public static readonly APLocation coin_isle2_secret = new APLocation("Inkwell Isle Two Secret Coin", 0xc4eb43);
        public static readonly APLocation coin_isle3_secret = new APLocation("Inkwell Isle Three Secret Coin", 0xc4eb44);
        public static readonly APLocation coin_isleh_secret = new APLocation("Inkwell Hell Secret Coin", 0xc4eb45);
        public static readonly APLocation dlc_coin_isle4_secret = new APLocation("Inkwell Isle Four Secret Coin", 0xc4fb05);
        public static readonly APLocation dlc_cookie = new APLocation("Astral Cookie", 0xc4fb03);
        public static readonly APLocation dlc_npc_newscat = new APLocation("Newsy Cat", 0xc4fb04);
        public static readonly APLocation dlc_quest_cactusgirl = new APLocation("Cactus Girl Quest", 0xc4fb07);
        public static readonly APLocation event_dlc_curse_complete = new APLocation("Event Divine Relic", 0xc4fb06);
        public static readonly APLocation level_boss_baroness = new APLocation("Sugarland Shimmy Complete", 0xc4eadb);
        public static readonly APLocation level_boss_baroness_dlc_chaliced = new APLocation("Sugarland Shimmy Chalice Complete", 0xc4fad6);
        public static readonly APLocation level_boss_baroness_topgrade = new APLocation("Sugarland Shimmy Top Grade", 0xc4eadc);
        public static readonly APLocation level_boss_bee = new APLocation("Honeycomb Herald Complete", 0xc4eae1);
        public static readonly APLocation level_boss_bee_dlc_chaliced = new APLocation("Honeycomb Herald Chalice Complete", 0xc4fad9);
        public static readonly APLocation level_boss_bee_topgrade = new APLocation("Honeycomb Herald Top Grade", 0xc4eae2);
        public static readonly APLocation level_boss_clown = new APLocation("Carnival Kerfuffle Complete", 0xc4eadd);
        public static readonly APLocation level_boss_clown_dlc_chaliced = new APLocation("Carnival Kerfuffle Chalice Complete", 0xc4fad7);
        public static readonly APLocation level_boss_clown_topgrade = new APLocation("Carnival Kerfuffle Top Grade", 0xc4eade);
        public static readonly APLocation level_boss_devil = new APLocation("One Hell of a Time Complete", 0xc4eaf7);
        public static readonly APLocation level_boss_devil_dlc_chaliced = new APLocation("One Hell of a Time Chalice Complete", 0xc4fae4);
        public static readonly APLocation level_boss_devil_topgrade = new APLocation("One Hell of a Time Top Grade", 0xc4eaf8);
        public static readonly APLocation level_boss_dragon = new APLocation("Fiery Frolic Complete", 0xc4eadf);
        public static readonly APLocation level_boss_dragon_dlc_chaliced = new APLocation("Fiery Frolic Chalice Complete", 0xc4fad8);
        public static readonly APLocation level_boss_dragon_topgrade = new APLocation("Fiery Frolic Top Grade", 0xc4eae0);
        public static readonly APLocation level_boss_flower = new APLocation("Floral Fury Complete", 0xc4ead9);
        public static readonly APLocation level_boss_flower_dlc_chaliced = new APLocation("Floral Fury Chalice Complete", 0xc4fad5);
        public static readonly APLocation level_boss_flower_topgrade = new APLocation("Floral Fury Top Grade", 0xc4eada);
        public static readonly APLocation level_boss_frogs = new APLocation("Clip Joint Calamity Complete", 0xc4ead7);
        public static readonly APLocation level_boss_frogs_dlc_chaliced = new APLocation("Clip Joint Calamity Chalice Complete", 0xc4fad4);
        public static readonly APLocation level_boss_frogs_topgrade = new APLocation("Clip Joint Calamity Top Grade", 0xc4ead8);
        public static readonly APLocation level_boss_kingdice = new APLocation("All Bets Are Off Complete", 0xc4eaeb);
        public static readonly APLocation level_boss_kingdice_dlc_chaliced = new APLocation("All Bets Are Off Chalice Complete", 0xc4fade);
        public static readonly APLocation level_boss_kingdice_topgrade = new APLocation("All Bets Are Off Top Grade", 0xc4eaec);
        public static readonly APLocation level_boss_mouse = new APLocation("Murine Corps Complete", 0xc4eae5);
        public static readonly APLocation level_boss_mouse_dlc_chaliced = new APLocation("Murine Corps Chalice Complete", 0xc4fadb);
        public static readonly APLocation level_boss_mouse_topgrade = new APLocation("Murine Corps Top Grade", 0xc4eae6);
        public static readonly APLocation level_boss_pirate = new APLocation("Shootin n' Lootin Complete", 0xc4eae3);
        public static readonly APLocation level_boss_pirate_dlc_chaliced = new APLocation("Shootin n' Lootin Chalice Complete", 0xc4fada);
        public static readonly APLocation level_boss_pirate_topgrade = new APLocation("Shootin n' Lootin Top Grade", 0xc4eae4);
        public static readonly APLocation level_boss_plane_bird = new APLocation("Aviary Action Complete", 0xc4eaf1);
        public static readonly APLocation level_boss_plane_bird_dlc_chaliced = new APLocation("Aviary Action Chalice Complete", 0xc4fae1);
        public static readonly APLocation level_boss_plane_bird_topgrade = new APLocation("Aviary Action Top Grade", 0xc4eaf2);
        public static readonly APLocation level_boss_plane_blimp = new APLocation("Threatenin' Zepplin Complete", 0xc4eaed);
        public static readonly APLocation level_boss_plane_blimp_dlc_chaliced = new APLocation("Threatenin' Zepplin Chalice Complete", 0xc4fadf);
        public static readonly APLocation level_boss_plane_blimp_topgrade = new APLocation("Threatenin' Zepplin Top Grade", 0xc4eaee);
        public static readonly APLocation level_boss_plane_genie = new APLocation("Pyramid Peril Complete", 0xc4eaef);
        public static readonly APLocation level_boss_plane_genie_dlc_chaliced = new APLocation("Pyramid Peril Chalice Complete", 0xc4fae0);
        public static readonly APLocation level_boss_plane_genie_topgrade = new APLocation("Pyramid Peril Top Grade", 0xc4eaf0);
        public static readonly APLocation level_boss_plane_mermaid = new APLocation("High Sea Hi-Jinx Complete", 0xc4eaf3);
        public static readonly APLocation level_boss_plane_mermaid_dlc_chaliced = new APLocation("High Sea Hi-Jinx Chalice Complete", 0xc4fae2);
        public static readonly APLocation level_boss_plane_mermaid_topgrade = new APLocation("High Sea Hi-Jinx Top Grade", 0xc4eaf4);
        public static readonly APLocation level_boss_plane_robot = new APLocation("Junkyard Jive Complete", 0xc4eaf5);
        public static readonly APLocation level_boss_plane_robot_dlc_chaliced = new APLocation("Junkyard Jive Chalice Complete", 0xc4fae3);
        public static readonly APLocation level_boss_plane_robot_topgrade = new APLocation("Junkyard Jive Top Grade", 0xc4eaf6);
        public static readonly APLocation level_boss_sallystageplay = new APLocation("Dramatic Fanatic Complete", 0xc4eae7);
        public static readonly APLocation level_boss_sallystageplay_dlc_chaliced = new APLocation("Dramatic Fanatic Chalice Complete", 0xc4fadc);
        public static readonly APLocation level_boss_sallystageplay_topgrade = new APLocation("Dramatic Fanatic Top Grade", 0xc4eae8);
        public static readonly APLocation level_boss_slime = new APLocation("Ruse of an Ooze Complete", 0xc4ead5);
        public static readonly APLocation level_boss_slime_dlc_chaliced = new APLocation("Ruse of an Ooze Chalice Complete", 0xc4fad3);
        public static readonly APLocation level_boss_slime_topgrade = new APLocation("Ruse of an Ooze Top Grade", 0xc4ead6);
        public static readonly APLocation level_boss_train = new APLocation("Railroad Wrath Complete", 0xc4eae9);
        public static readonly APLocation level_boss_train_dlc_chaliced = new APLocation("Railroad Wrath Chalice Complete", 0xc4fadd);
        public static readonly APLocation level_boss_train_topgrade = new APLocation("Railroad Wrath Top Grade", 0xc4eaea);
        public static readonly APLocation level_boss_veggies = new APLocation("Botanic Panic Complete", 0xc4ead3);
        public static readonly APLocation level_boss_veggies_dlc_chaliced = new APLocation("Botanic Panic Chalice Complete", 0xc4fad2);
        public static readonly APLocation level_boss_veggies_topgrade = new APLocation("Botanic Panic Top Grade", 0xc4ead4);
        public static readonly APLocation level_dicepalace_boss_booze = new APLocation("Kingdice Miniboss 1 Complete", 0xc4eaf9);
        public static readonly APLocation level_dicepalace_boss_chips = new APLocation("Kingdice Miniboss 2 Complete", 0xc4eafa);
        public static readonly APLocation level_dicepalace_boss_cigar = new APLocation("Kingdice Miniboss 3 Complete", 0xc4eafb);
        public static readonly APLocation level_dicepalace_boss_domino = new APLocation("Kingdice Miniboss 4 Complete", 0xc4eafc);
        public static readonly APLocation level_dicepalace_boss_eightball = new APLocation("Kingdice Miniboss 8 Complete", 0xc4eb00);
        public static readonly APLocation level_dicepalace_boss_plane_horse = new APLocation("Kingdice Miniboss 6 Complete", 0xc4eafe);
        public static readonly APLocation level_dicepalace_boss_plane_memory = new APLocation("Kingdice Miniboss 9 Complete", 0xc4eb01);
        public static readonly APLocation level_dicepalace_boss_rabbit = new APLocation("Kingdice Miniboss 5 Complete", 0xc4eafd);
        public static readonly APLocation level_dicepalace_boss_roulette = new APLocation("Kingdice Miniboss 7 Complete", 0xc4eaff);
        public static readonly APLocation level_dlc_boss_airplane = new APLocation("Doggone Dogfight Complete", 0xc4faee);
        public static readonly APLocation level_dlc_boss_airplane_dlc_chaliced = new APLocation("Doggone Dogfight Chalice Complete", 0xc4faf0);
        public static readonly APLocation level_dlc_boss_airplane_topgrade = new APLocation("Doggone Dogfight Top Grade", 0xc4faef);
        public static readonly APLocation level_dlc_boss_oldman = new APLocation("Gnome Way Out Complete", 0xc4fae5);
        public static readonly APLocation level_dlc_boss_oldman_dlc_chaliced = new APLocation("Gnome Way Out Chalice Complete", 0xc4fae7);
        public static readonly APLocation level_dlc_boss_oldman_topgrade = new APLocation("Gnome Way Out Top Grade", 0xc4fae6);
        public static readonly APLocation level_dlc_boss_plane_cowboy = new APLocation("High-Noon Hoopla Complete", 0xc4faf1);
        public static readonly APLocation level_dlc_boss_plane_cowboy_dlc_chaliced = new APLocation("High-Noon Hoopla Chalice Complete", 0xc4faf3);
        public static readonly APLocation level_dlc_boss_plane_cowboy_topgrade = new APLocation("High-Noon Hoopla Top Grade", 0xc4faf2);
        public static readonly APLocation level_dlc_boss_rumrunners = new APLocation("Bootlegger Boogie Complete", 0xc4fae8);
        public static readonly APLocation level_dlc_boss_rumrunners_dlc_chaliced = new APLocation("Bootlegger Boogie Chalice Complete", 0xc4faea);
        public static readonly APLocation level_dlc_boss_rumrunners_topgrade = new APLocation("Bootlegger Boogie Top Grade", 0xc4fae9);
        public static readonly APLocation level_dlc_boss_saltbaker_dlc_chaliced = new APLocation("A Dish to Die For Chalice Complete", 0xc4faf6);
        public static readonly APLocation level_dlc_boss_snowcult = new APLocation("Snow Cult Scuffle Complete", 0xc4faeb);
        public static readonly APLocation level_dlc_boss_snowcult_dlc_chaliced = new APLocation("Snow Cult Scuffle Chalice Complete", 0xc4faed);
        public static readonly APLocation level_dlc_boss_snowcult_topgrade = new APLocation("Snow Cult Scuffle Top Grade", 0xc4faec);
        public static readonly APLocation level_dlc_chesscastle_bishop = new APLocation("The Bishop Reward", 0xc4fafa);
        public static readonly APLocation level_dlc_chesscastle_knight = new APLocation("The Knight Reward", 0xc4faf9);
        public static readonly APLocation level_dlc_chesscastle_pawn = new APLocation("The Pawns Reward", 0xc4faf8);
        public static readonly APLocation level_dlc_chesscastle_queen = new APLocation("The Queen Reward", 0xc4fafc);
        public static readonly APLocation level_dlc_chesscastle_rook = new APLocation("The Rook Reward", 0xc4fafb);
        public static readonly APLocation level_dlc_chesscastle_run = new APLocation("The King's Leap Full Run Complete", 0xc4faf7);
        public static readonly APLocation level_dlc_graveyard = new APLocation("Graveyard Dream Complete", 0xc4fafd);
        public static readonly APLocation level_dlc_tutorial = new APLocation("Recipe for Ms. Chalice Complete", 0xc4fad0);
        public static readonly APLocation level_dlc_tutorial_coin = new APLocation("Recipe for Ms. Chalice Coin", 0xc4fad1);
        public static readonly APLocation level_mausoleum_i = new APLocation("Mausoleum I", 0xc4eb32);
        public static readonly APLocation level_mausoleum_ii = new APLocation("Mausoleum II", 0xc4eb33);
        public static readonly APLocation level_mausoleum_iii = new APLocation("Mausoleum III", 0xc4eb34);
        public static readonly APLocation level_plane_tutorial = new APLocation("Plane Tutorial Complete", 0xc4ead2);
        public static readonly APLocation level_rungun_circus = new APLocation("Funfair Fever Complete", 0xc4eb12);
        public static readonly APLocation level_rungun_circus_agrade = new APLocation("Funfair Fever Top Grade", 0xc4eb13);
        public static readonly APLocation level_rungun_circus_coin1 = new APLocation("Funfair Fever Coin 1", 0xc4eb15);
        public static readonly APLocation level_rungun_circus_coin2 = new APLocation("Funfair Fever Coin 2", 0xc4eb16);
        public static readonly APLocation level_rungun_circus_coin3 = new APLocation("Funfair Fever Coin 3", 0xc4eb17);
        public static readonly APLocation level_rungun_circus_coin4 = new APLocation("Funfair Fever Coin 4", 0xc4eb18);
        public static readonly APLocation level_rungun_circus_coin5 = new APLocation("Funfair Fever Coin 5", 0xc4eb19);
        public static readonly APLocation level_rungun_circus_pacifist = new APLocation("Funfair Fever Pacifist", 0xc4eb14);
        public static readonly APLocation level_rungun_forest = new APLocation("Forest Follies Complete", 0xc4eb02);
        public static readonly APLocation level_rungun_forest_agrade = new APLocation("Forest Follies Top Grade", 0xc4eb03);
        public static readonly APLocation level_rungun_forest_coin1 = new APLocation("Forest Follies Coin 1", 0xc4eb05);
        public static readonly APLocation level_rungun_forest_coin2 = new APLocation("Forest Follies Coin 2", 0xc4eb06);
        public static readonly APLocation level_rungun_forest_coin3 = new APLocation("Forest Follies Coin 3", 0xc4eb07);
        public static readonly APLocation level_rungun_forest_coin4 = new APLocation("Forest Follies Coin 4", 0xc4eb08);
        public static readonly APLocation level_rungun_forest_coin5 = new APLocation("Forest Follies Coin 5", 0xc4eb09);
        public static readonly APLocation level_rungun_forest_pacifist = new APLocation("Forest Follies Pacifist", 0xc4eb04);
        public static readonly APLocation level_rungun_funhouse = new APLocation("Funhouse Frazzle Complete", 0xc4eb1a);
        public static readonly APLocation level_rungun_funhouse_agrade = new APLocation("Funhouse Frazzle Top Grade", 0xc4eb1b);
        public static readonly APLocation level_rungun_funhouse_coin1 = new APLocation("Funhouse Frazzle Coin 1", 0xc4eb1d);
        public static readonly APLocation level_rungun_funhouse_coin2 = new APLocation("Funhouse Frazzle Coin 2", 0xc4eb1e);
        public static readonly APLocation level_rungun_funhouse_coin3 = new APLocation("Funhouse Frazzle Coin 3", 0xc4eb1f);
        public static readonly APLocation level_rungun_funhouse_coin4 = new APLocation("Funhouse Frazzle Coin 4", 0xc4eb20);
        public static readonly APLocation level_rungun_funhouse_coin5 = new APLocation("Funhouse Frazzle Coin 5", 0xc4eb21);
        public static readonly APLocation level_rungun_funhouse_pacifist = new APLocation("Funhouse Frazzle Pacifist", 0xc4eb1c);
        public static readonly APLocation level_rungun_harbour = new APLocation("Perilous Piers Complete", 0xc4eb22);
        public static readonly APLocation level_rungun_harbour_agrade = new APLocation("Perilous Piers Top Grade", 0xc4eb23);
        public static readonly APLocation level_rungun_harbour_coin1 = new APLocation("Perilous Piers Coin 1", 0xc4eb25);
        public static readonly APLocation level_rungun_harbour_coin2 = new APLocation("Perilous Piers Coin 2", 0xc4eb26);
        public static readonly APLocation level_rungun_harbour_coin3 = new APLocation("Perilous Piers Coin 3", 0xc4eb27);
        public static readonly APLocation level_rungun_harbour_coin4 = new APLocation("Perilous Piers Coin 4", 0xc4eb28);
        public static readonly APLocation level_rungun_harbour_coin5 = new APLocation("Perilous Piers Coin 5", 0xc4eb29);
        public static readonly APLocation level_rungun_harbour_pacifist = new APLocation("Perilous Piers Pacifist", 0xc4eb24);
        public static readonly APLocation level_rungun_mountain = new APLocation("Rugged Ridge Complete", 0xc4eb2a);
        public static readonly APLocation level_rungun_mountain_agrade = new APLocation("Rugged Ridge Top Grade", 0xc4eb2b);
        public static readonly APLocation level_rungun_mountain_coin1 = new APLocation("Rugged Ridge Coin 1", 0xc4eb2d);
        public static readonly APLocation level_rungun_mountain_coin2 = new APLocation("Rugged Ridge Coin 2", 0xc4eb2e);
        public static readonly APLocation level_rungun_mountain_coin3 = new APLocation("Rugged Ridge Coin 3", 0xc4eb2f);
        public static readonly APLocation level_rungun_mountain_coin4 = new APLocation("Rugged Ridge Coin 4", 0xc4eb30);
        public static readonly APLocation level_rungun_mountain_coin5 = new APLocation("Rugged Ridge Coin 5", 0xc4eb31);
        public static readonly APLocation level_rungun_mountain_pacifist = new APLocation("Rugged Ridge Pacifist", 0xc4eb2c);
        public static readonly APLocation level_rungun_tree = new APLocation("Treetop Trouble Complete", 0xc4eb0a);
        public static readonly APLocation level_rungun_tree_agrade = new APLocation("Treetop Trouble Top Grade", 0xc4eb0b);
        public static readonly APLocation level_rungun_tree_coin1 = new APLocation("Treetop Trouble Coin 1", 0xc4eb0d);
        public static readonly APLocation level_rungun_tree_coin2 = new APLocation("Treetop Trouble Coin 2", 0xc4eb0e);
        public static readonly APLocation level_rungun_tree_coin3 = new APLocation("Treetop Trouble Coin 3", 0xc4eb0f);
        public static readonly APLocation level_rungun_tree_coin4 = new APLocation("Treetop Trouble Coin 4", 0xc4eb10);
        public static readonly APLocation level_rungun_tree_coin5 = new APLocation("Treetop Trouble Coin 5", 0xc4eb11);
        public static readonly APLocation level_rungun_tree_pacifist = new APLocation("Treetop Trouble Pacifist", 0xc4eb0c);
        public static readonly APLocation level_tutorial = new APLocation("Tutorial Complete", 0xc4ead0);
        public static readonly APLocation level_tutorial_coin = new APLocation("Tutorial Coin", 0xc4ead1);
        public static readonly APLocation npc_canteen = new APLocation("Canteen Hughes", 0xc4eb41);
        public static readonly APLocation npc_mac = new APLocation("Mac", 0xc4eb40);
        public static readonly APLocation quest_15agrades = new APLocation("15+ Grade-A Quest", 0xc4eb4b);
        public static readonly APLocation quest_4mel = new APLocation("Barbershop Quartet Quest", 0xc4eb48);
        public static readonly APLocation quest_4parries = new APLocation("Buster Quest", 0xc4eb46);
        public static readonly APLocation quest_ginger = new APLocation("Ginger Quest", 0xc4eb47);
        public static readonly APLocation quest_lucien = new APLocation("Lucien Quest", 0xc4eb49);
        public static readonly APLocation quest_ludwig = new APLocation("Ludwig Quest", 0xc4eb4c);
        public static readonly APLocation quest_pacifist = new APLocation("Pacifist Quest", 0xc4eb4a);
        public static readonly APLocation quest_wolfgang = new APLocation("Wolfgang Quest", 0xc4eb4d);
        public static readonly APLocation shop_charm1 = new APLocation("Porkrind's Emporium Charm 1", 0xc4eb3a);
        public static readonly APLocation shop_charm2 = new APLocation("Porkrind's Emporium Charm 2", 0xc4eb3b);
        public static readonly APLocation shop_charm3 = new APLocation("Porkrind's Emporium Charm 3", 0xc4eb3c);
        public static readonly APLocation shop_charm4 = new APLocation("Porkrind's Emporium Charm 4", 0xc4eb3d);
        public static readonly APLocation shop_charm5 = new APLocation("Porkrind's Emporium Charm 5", 0xc4eb3e);
        public static readonly APLocation shop_charm6 = new APLocation("Porkrind's Emporium Charm 6", 0xc4eb3f);
        public static readonly APLocation shop_dlc_charm7 = new APLocation("Porkrind's Emporium Charm 7", 0xc4fb01);
        public static readonly APLocation shop_dlc_charm8 = new APLocation("Porkrind's Emporium Charm 8", 0xc4fb02);
        public static readonly APLocation shop_dlc_weapon6 = new APLocation("Porkrind's Emporium Weapon 6", 0xc4fafe);
        public static readonly APLocation shop_dlc_weapon7 = new APLocation("Porkrind's Emporium Weapon 7", 0xc4faff);
        public static readonly APLocation shop_dlc_weapon8 = new APLocation("Porkrind's Emporium Weapon 8", 0xc4fb00);
        public static readonly APLocation shop_weapon1 = new APLocation("Porkrind's Emporium Weapon 1", 0xc4eb35);
        public static readonly APLocation shop_weapon2 = new APLocation("Porkrind's Emporium Weapon 2", 0xc4eb36);
        public static readonly APLocation shop_weapon3 = new APLocation("Porkrind's Emporium Weapon 3", 0xc4eb37);
        public static readonly APLocation shop_weapon4 = new APLocation("Porkrind's Emporium Weapon 4", 0xc4eb38);
        public static readonly APLocation shop_weapon5 = new APLocation("Porkrind's Emporium Weapon 5", 0xc4eb39);

        public static APLocation FromId(long id) => new APLocation(id_to_name[id], id);
        public static bool IdExists(long id) => id_to_name.ContainsKey(id);
        public static bool NameExists(string name) => id_to_name.ContainsValue(name);
        public static string IdToName(long id) => id_to_name[id];
        public static long NameToId(string name) => name_to_id[name];
    }
}
