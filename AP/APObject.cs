/* Generated by GenerateCS_APObject */

using System.Collections.Generic;

namespace CupheadArchipelago.AP {
    public abstract class APObject {
        public long Id { get; private set; }

        public APObject(long id) {
            Id = id;
        }

        public static implicit operator long(APObject a) => a.Id;
        public override string ToString() { return $"APObject {Id}"; }
    }

    public class APItem : APObject {
        private static readonly Dictionary<long,APItem> id_map = new();
        public APItem(long id) : base(id) { id_map.Add(id,this); }

        public static readonly APItem level_generic = new APItem(12905168);
        public static readonly APItem level_extrahealth = new APItem(12905169);
        public static readonly APItem level_superrecharge = new APItem(12905170);
        public static readonly APItem level_fastfire = new APItem(12905171);
        public static readonly APItem coin = new APItem(12905173);
        public static readonly APItem coin2 = new APItem(12905174);
        public static readonly APItem coin3 = new APItem(12905175);
        public static readonly APItem plane_gun = new APItem(12905176);
        public static readonly APItem plane_bombs = new APItem(12905177);
        public static readonly APItem plane_super = new APItem(12905178);
        public static readonly APItem contract = new APItem(12905179);
        public static readonly APItem healthupgrade = new APItem(12905180);
        public static readonly APItem weapon_peashooter = new APItem(12905181);
        public static readonly APItem weapon_spread = new APItem(12905182);
        public static readonly APItem weapon_chaser = new APItem(12905183);
        public static readonly APItem weapon_lobber = new APItem(12905184);
        public static readonly APItem weapon_charge = new APItem(12905185);
        public static readonly APItem weapon_roundabout = new APItem(12905186);
        public static readonly APItem charm_heart = new APItem(12905187);
        public static readonly APItem charm_smokebomb = new APItem(12905188);
        public static readonly APItem charm_psugar = new APItem(12905189);
        public static readonly APItem charm_coffee = new APItem(12905190);
        public static readonly APItem charm_twinheart = new APItem(12905191);
        public static readonly APItem charm_whetstone = new APItem(12905192);
        public static readonly APItem super_i = new APItem(12905193);
        public static readonly APItem super_ii = new APItem(12905194);
        public static readonly APItem super_iii = new APItem(12905195);
        public static readonly APItem ability_duck = new APItem(12905196);
        public static readonly APItem ability_dash = new APItem(12905197);
        public static readonly APItem ability_parry = new APItem(12905198);
        public static readonly APItem ability_plane_shrink = new APItem(12905199);
        public static readonly APItem ability_plane_parry = new APItem(12905200);
        public static readonly APItem ability_aim_left = new APItem(12905201);
        public static readonly APItem ability_aim_right = new APItem(12905202);
        public static readonly APItem ability_aim_up = new APItem(12905203);
        public static readonly APItem ability_aim_down = new APItem(12905204);
        public static readonly APItem ability_aim_upleft = new APItem(12905205);
        public static readonly APItem ability_aim_upright = new APItem(12905206);
        public static readonly APItem ability_aim_downleft = new APItem(12905207);
        public static readonly APItem ability_aim_downright = new APItem(12905208);
        public static readonly APItem level_trap_fingerjam = new APItem(12905209);
        public static readonly APItem level_trap_slowfire = new APItem(12905210);
        public static readonly APItem level_trap_superdrain = new APItem(12905211);
        public static readonly APItem level_trap_reverse = new APItem(12905212);
        public static readonly APItem level_trap_screen = new APItem(12905213);
        public static readonly APItem dlc_boat = new APItem(12909264);
        public static readonly APItem dlc_ingredient = new APItem(12909265);
        public static readonly APItem weapon_dlc_crackshot = new APItem(12909266);
        public static readonly APItem weapon_dlc_converge = new APItem(12909267);
        public static readonly APItem weapon_dlc_twistup = new APItem(12909268);
        public static readonly APItem charm_dlc_heartring = new APItem(12909270);
        public static readonly APItem charm_dlc_broken_relic = new APItem(12909271);

        public static APItem FromId(long id) => id_map[id];
        public static bool IdExists(long id) => id_map.ContainsKey(id);
        public override string ToString() { return $"APItem {Id}"; }
    }

    public class APLocation : APObject {
        private static readonly Dictionary<long,APLocation> id_map = new();
        public APLocation(long id) : base(id) { id_map.Add(id,this); }

        public static readonly APLocation level_tutorial = new APLocation(12905168);
        public static readonly APLocation level_tutorial_coin = new APLocation(12905169);
        public static readonly APLocation level_boss_veggies = new APLocation(12905171);
        public static readonly APLocation level_boss_veggies_topgrade = new APLocation(12905172);
        public static readonly APLocation level_boss_slime = new APLocation(12905173);
        public static readonly APLocation level_boss_slime_topgrade = new APLocation(12905174);
        public static readonly APLocation level_boss_frogs = new APLocation(12905175);
        public static readonly APLocation level_boss_frogs_topgrade = new APLocation(12905176);
        public static readonly APLocation level_boss_flower = new APLocation(12905177);
        public static readonly APLocation level_boss_flower_topgrade = new APLocation(12905178);
        public static readonly APLocation level_boss_baroness = new APLocation(12905179);
        public static readonly APLocation level_boss_baroness_topgrade = new APLocation(12905180);
        public static readonly APLocation level_boss_clown = new APLocation(12905181);
        public static readonly APLocation level_boss_clown_topgrade = new APLocation(12905182);
        public static readonly APLocation level_boss_dragon = new APLocation(12905183);
        public static readonly APLocation level_boss_dragon_topgrade = new APLocation(12905184);
        public static readonly APLocation level_boss_bee = new APLocation(12905185);
        public static readonly APLocation level_boss_bee_topgrade = new APLocation(12905186);
        public static readonly APLocation level_boss_pirate = new APLocation(12905187);
        public static readonly APLocation level_boss_pirate_topgrade = new APLocation(12905188);
        public static readonly APLocation level_boss_mouse = new APLocation(12905189);
        public static readonly APLocation level_boss_mouse_topgrade = new APLocation(12905190);
        public static readonly APLocation level_boss_sallystageplay = new APLocation(12905191);
        public static readonly APLocation level_boss_sallystageplay_topgrade = new APLocation(12905192);
        public static readonly APLocation level_boss_train = new APLocation(12905193);
        public static readonly APLocation level_boss_train_topgrade = new APLocation(12905194);
        public static readonly APLocation level_boss_kingdice = new APLocation(12905195);
        public static readonly APLocation level_boss_kingdice_topgrade = new APLocation(12905196);
        public static readonly APLocation level_boss_plane_blimp = new APLocation(12905197);
        public static readonly APLocation level_boss_plane_blimp_topgrade = new APLocation(12905198);
        public static readonly APLocation level_boss_plane_genie = new APLocation(12905199);
        public static readonly APLocation level_boss_plane_genie_topgrade = new APLocation(12905200);
        public static readonly APLocation level_boss_plane_bird = new APLocation(12905201);
        public static readonly APLocation level_boss_plane_bird_topgrade = new APLocation(12905202);
        public static readonly APLocation level_boss_plane_mermaid = new APLocation(12905203);
        public static readonly APLocation level_boss_plane_mermaid_topgrade = new APLocation(12905204);
        public static readonly APLocation level_boss_plane_robot = new APLocation(12905205);
        public static readonly APLocation level_boss_plane_robot_topgrade = new APLocation(12905206);
        public static readonly APLocation level_dicepalace_boss_booze = new APLocation(12905209);
        public static readonly APLocation level_dicepalace_boss_chips = new APLocation(12905210);
        public static readonly APLocation level_dicepalace_boss_cigar = new APLocation(12905211);
        public static readonly APLocation level_dicepalace_boss_domino = new APLocation(12905212);
        public static readonly APLocation level_dicepalace_boss_rabbit = new APLocation(12905213);
        public static readonly APLocation level_dicepalace_boss_plane_horse = new APLocation(12905214);
        public static readonly APLocation level_dicepalace_boss_roulette = new APLocation(12905215);
        public static readonly APLocation level_dicepalace_boss_eightball = new APLocation(12905216);
        public static readonly APLocation level_dicepalace_boss_plane_memory = new APLocation(12905217);
        public static readonly APLocation level_rungun_forest = new APLocation(12905218);
        public static readonly APLocation level_rungun_forest_agrade = new APLocation(12905219);
        public static readonly APLocation level_rungun_forest_pacifist = new APLocation(12905220);
        public static readonly APLocation level_rungun_forest_coin1 = new APLocation(12905221);
        public static readonly APLocation level_rungun_forest_coin2 = new APLocation(12905222);
        public static readonly APLocation level_rungun_forest_coin3 = new APLocation(12905223);
        public static readonly APLocation level_rungun_forest_coin4 = new APLocation(12905224);
        public static readonly APLocation level_rungun_forest_coin5 = new APLocation(12905225);
        public static readonly APLocation level_rungun_tree = new APLocation(12905226);
        public static readonly APLocation level_rungun_tree_agrade = new APLocation(12905227);
        public static readonly APLocation level_rungun_tree_pacifist = new APLocation(12905228);
        public static readonly APLocation level_rungun_tree_coin1 = new APLocation(12905229);
        public static readonly APLocation level_rungun_tree_coin2 = new APLocation(12905230);
        public static readonly APLocation level_rungun_tree_coin3 = new APLocation(12905231);
        public static readonly APLocation level_rungun_tree_coin4 = new APLocation(12905232);
        public static readonly APLocation level_rungun_tree_coin5 = new APLocation(12905233);
        public static readonly APLocation level_rungun_circus = new APLocation(12905234);
        public static readonly APLocation level_rungun_circus_agrade = new APLocation(12905235);
        public static readonly APLocation level_rungun_circus_pacifist = new APLocation(12905236);
        public static readonly APLocation level_rungun_circus_coin1 = new APLocation(12905237);
        public static readonly APLocation level_rungun_circus_coin2 = new APLocation(12905238);
        public static readonly APLocation level_rungun_circus_coin3 = new APLocation(12905239);
        public static readonly APLocation level_rungun_circus_coin4 = new APLocation(12905240);
        public static readonly APLocation level_rungun_circus_coin5 = new APLocation(12905241);
        public static readonly APLocation level_rungun_funhouse = new APLocation(12905242);
        public static readonly APLocation level_rungun_funhouse_agrade = new APLocation(12905243);
        public static readonly APLocation level_rungun_funhouse_pacifist = new APLocation(12905244);
        public static readonly APLocation level_rungun_funhouse_coin1 = new APLocation(12905245);
        public static readonly APLocation level_rungun_funhouse_coin2 = new APLocation(12905246);
        public static readonly APLocation level_rungun_funhouse_coin3 = new APLocation(12905247);
        public static readonly APLocation level_rungun_funhouse_coin4 = new APLocation(12905248);
        public static readonly APLocation level_rungun_funhouse_coin5 = new APLocation(12905249);
        public static readonly APLocation level_rungun_harbour = new APLocation(12905250);
        public static readonly APLocation level_rungun_harbour_agrade = new APLocation(12905251);
        public static readonly APLocation level_rungun_harbour_pacifist = new APLocation(12905252);
        public static readonly APLocation level_rungun_harbour_coin1 = new APLocation(12905253);
        public static readonly APLocation level_rungun_harbour_coin2 = new APLocation(12905254);
        public static readonly APLocation level_rungun_harbour_coin3 = new APLocation(12905255);
        public static readonly APLocation level_rungun_harbour_coin4 = new APLocation(12905256);
        public static readonly APLocation level_rungun_harbour_coin5 = new APLocation(12905257);
        public static readonly APLocation level_rungun_mountain = new APLocation(12905258);
        public static readonly APLocation level_rungun_mountain_agrade = new APLocation(12905259);
        public static readonly APLocation level_rungun_mountain_pacifist = new APLocation(12905260);
        public static readonly APLocation level_rungun_mountain_coin1 = new APLocation(12905261);
        public static readonly APLocation level_rungun_mountain_coin2 = new APLocation(12905262);
        public static readonly APLocation level_rungun_mountain_coin3 = new APLocation(12905263);
        public static readonly APLocation level_rungun_mountain_coin4 = new APLocation(12905264);
        public static readonly APLocation level_rungun_mountain_coin5 = new APLocation(12905265);
        public static readonly APLocation level_mausoleum_i = new APLocation(12905266);
        public static readonly APLocation level_mausoleum_ii = new APLocation(12905267);
        public static readonly APLocation level_mausoleum_iii = new APLocation(12905268);
        public static readonly APLocation shop_weapon1 = new APLocation(12905269);
        public static readonly APLocation shop_weapon2 = new APLocation(12905270);
        public static readonly APLocation shop_weapon3 = new APLocation(12905271);
        public static readonly APLocation shop_weapon4 = new APLocation(12905272);
        public static readonly APLocation shop_weapon5 = new APLocation(12905273);
        public static readonly APLocation shop_charm1 = new APLocation(12905274);
        public static readonly APLocation shop_charm2 = new APLocation(12905275);
        public static readonly APLocation shop_charm3 = new APLocation(12905276);
        public static readonly APLocation shop_charm4 = new APLocation(12905277);
        public static readonly APLocation shop_charm5 = new APLocation(12905278);
        public static readonly APLocation shop_charm6 = new APLocation(12905279);
        public static readonly APLocation npc_mac = new APLocation(12905280);
        public static readonly APLocation npc_canteen = new APLocation(12905281);
        public static readonly APLocation coin_isle1_secret = new APLocation(12905282);
        public static readonly APLocation coin_isle2_secret = new APLocation(12905283);
        public static readonly APLocation coin_isle3_secret = new APLocation(12905284);
        public static readonly APLocation coin_isleh_secret = new APLocation(12905285);
        public static readonly APLocation quest_4parries = new APLocation(12905286);
        public static readonly APLocation quest_ginger = new APLocation(12905287);
        public static readonly APLocation quest_4mel = new APLocation(12905288);
        public static readonly APLocation quest_lucien = new APLocation(12905289);
        public static readonly APLocation quest_pacifist = new APLocation(12905290);
        public static readonly APLocation quest_silverworth = new APLocation(12905291);
        public static readonly APLocation quest_music = new APLocation(12905292);
        public static readonly APLocation level_dlc_tutorial = new APLocation(12909264);
        public static readonly APLocation level_dlc_tutorial_coin = new APLocation(12909265);
        public static readonly APLocation level_boss_veggies_dlc_chaliced = new APLocation(12909266);
        public static readonly APLocation level_boss_slime_dlc_chaliced = new APLocation(12909267);
        public static readonly APLocation level_boss_frogs_dlc_chaliced = new APLocation(12909268);
        public static readonly APLocation level_boss_flower_dlc_chaliced = new APLocation(12909269);
        public static readonly APLocation level_boss_baroness_dlc_chaliced = new APLocation(12909270);
        public static readonly APLocation level_boss_clown_dlc_chaliced = new APLocation(12909271);
        public static readonly APLocation level_boss_dragon_dlc_chaliced = new APLocation(12909272);
        public static readonly APLocation level_boss_bee_dlc_chaliced = new APLocation(12909273);
        public static readonly APLocation level_boss_pirate_dlc_chaliced = new APLocation(12909274);
        public static readonly APLocation level_boss_mouse_dlc_chaliced = new APLocation(12909275);
        public static readonly APLocation level_boss_sallystageplay_dlc_chaliced = new APLocation(12909276);
        public static readonly APLocation level_boss_train_dlc_chaliced = new APLocation(12909277);
        public static readonly APLocation level_boss_kingdice_dlc_chaliced = new APLocation(12909278);
        public static readonly APLocation level_boss_plane_blimp_dlc_chaliced = new APLocation(12909279);
        public static readonly APLocation level_boss_plane_genie_dlc_chaliced = new APLocation(12909280);
        public static readonly APLocation level_boss_plane_bird_dlc_chaliced = new APLocation(12909281);
        public static readonly APLocation level_boss_plane_mermaid_dlc_chaliced = new APLocation(12909282);
        public static readonly APLocation level_boss_plane_robot_dlc_chaliced = new APLocation(12909283);
        public static readonly APLocation level_dlc_boss_oldman = new APLocation(12909285);
        public static readonly APLocation level_dlc_boss_oldman_topgrade = new APLocation(12909286);
        public static readonly APLocation level_dlc_boss_oldman_dlc_chaliced = new APLocation(12909287);
        public static readonly APLocation level_dlc_boss_rumrunners = new APLocation(12909288);
        public static readonly APLocation level_dlc_boss_rumrunners_topgrade = new APLocation(12909289);
        public static readonly APLocation level_dlc_boss_rumrunners_dlc_chaliced = new APLocation(12909290);
        public static readonly APLocation level_dlc_boss_snowcult = new APLocation(12909291);
        public static readonly APLocation level_dlc_boss_snowcult_topgrade = new APLocation(12909292);
        public static readonly APLocation level_dlc_boss_snowcult_dlc_chaliced = new APLocation(12909293);
        public static readonly APLocation level_dlc_boss_airplane = new APLocation(12909294);
        public static readonly APLocation level_dlc_boss_airplane_topgrade = new APLocation(12909295);
        public static readonly APLocation level_dlc_boss_airplane_dlc_chaliced = new APLocation(12909296);
        public static readonly APLocation level_dlc_boss_plane_cowboy = new APLocation(12909297);
        public static readonly APLocation level_dlc_boss_plane_cowboy_topgrade = new APLocation(12909298);
        public static readonly APLocation level_dlc_boss_plane_cowboy_dlc_chaliced = new APLocation(12909299);
        public static readonly APLocation level_dlc_chesscastle_run = new APLocation(12909303);
        public static readonly APLocation level_dlc_chesscastle_pawn = new APLocation(12909304);
        public static readonly APLocation level_dlc_chesscastle_knight = new APLocation(12909305);
        public static readonly APLocation level_dlc_chesscastle_bishop = new APLocation(12909306);
        public static readonly APLocation level_dlc_chesscastle_rook = new APLocation(12909307);
        public static readonly APLocation level_dlc_chesscastle_queen = new APLocation(12909308);
        public static readonly APLocation level_dlc_graveyard = new APLocation(12909309);
        public static readonly APLocation shop_dlc_weapon6 = new APLocation(12909310);
        public static readonly APLocation shop_dlc_weapon7 = new APLocation(12909311);
        public static readonly APLocation shop_dlc_weapon8 = new APLocation(12909312);
        public static readonly APLocation shop_dlc_charm7 = new APLocation(12909313);
        public static readonly APLocation shop_dlc_charm8 = new APLocation(12909314);
        public static readonly APLocation dlc_npc_newscat = new APLocation(12909316);
        public static readonly APLocation dlc_coin_isle4_secret = new APLocation(12909317);
        public static readonly APLocation dlc_quest_cactusgirl = new APLocation(12909319);

        public static APLocation FromId(long id) => id_map[id];
        public static bool IdExists(long id) => id_map.ContainsKey(id);
        public override string ToString() { return $"APLocation {Id}"; }
    }
}
