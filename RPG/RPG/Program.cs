internal class Program
{
    private const int MonstersToDefeat = 10;

    private static Dictionary<HeroClass, HeroCharacteristics> _heroCharacteristics = new Dictionary<HeroClass, HeroCharacteristics>();
    private static Hero? _hero;
    private static int _fullHealthRestorationsCount = 3;

    private static void Main()
    {
        FillCharacteristics();

        SetupHero();
        
        EnterGameLoop();
    }

    private static void EnterGameLoop()
    {
        int monstersDefeated = 0;

        while (monstersDefeated < MonstersToDefeat && !_hero.IsDead)
        {
            var monster = Monster.CreateRandomMonster();
            
            Console.WriteLine($"Monster created. Monsters to defeat: {MonstersToDefeat - monstersDefeated}");

            while (!_hero.IsDead && !monster.IsDead)
            {
                Console.WriteLine("Choose action. 1 - attack, 2 - heal");
                var heroAction = Console.ReadLine();
                
                if (!int.TryParse(heroAction, out var heroActionNumber) || heroActionNumber > 2 || heroActionNumber < 1)
                {
                    Console.WriteLine("Wrong input");
                    continue;
                }

                switch (heroActionNumber)
                {
                    case 1:
                        var attackDamage = _hero.GetAttackDamage();
                        
                        monster.TakeDamage(attackDamage);
                        
                        break;
                    case 2:
                        _hero.Heal();
                        
                        break;
                }

                if (monster.IsDead)
                {
                    Console.WriteLine("Monster defeated");
                    
                    monstersDefeated++;
                    
                    _hero.GainExperience();
                    
                    break;
                }
                
                var monsterAttackDamage = monster.GetAttackPower();
                
                _hero.TakeDamage(monsterAttackDamage);
                
                Console.WriteLine($"Monster attacked hero");

                if (_hero.IsDead)
                {
                    Console.WriteLine("Hero defeated");
                    
                    break;
                }
            }
        }
        
        Console.WriteLine(_hero.IsDead ? "Game over. Hero defeated." : "All monsters defeated");
    }

    private static void SetupHero()
    {
        Console.WriteLine("Choose hero class. 1 - warrior, 2 - mage, 3 - archer");
        
        var chosenClass = Console.ReadLine();

        if (!int.TryParse(chosenClass, out var chosenClassNumber))
        {
            Console.WriteLine("Wrong input");
            return;
        }

        HeroClass chosenClassValue = default;

        switch (chosenClassNumber)
        {
            case 1:
                chosenClassValue = HeroClass.Warrior;
                break;
            case 2:
                chosenClassValue = HeroClass.Mage;
                break;
            case 3:
                chosenClassValue = HeroClass.Archer;
                break;
            default:
                Console.WriteLine("Wrong input");
                break;
        }
        
        var chosenHeroCharacteristics = _heroCharacteristics[chosenClassValue];
        
        _hero = new Hero("Mighty Hero", chosenHeroCharacteristics);
        
        _hero.OnLevelUp += HandleLevelUp;
        
        Console.WriteLine($"Chosen hero: {_hero.Name} with class {chosenClassValue}");
    }

    private static void HandleLevelUp()
    {
        Console.WriteLine("Choose characteristic to improve. 1 is maxHealth, 2 is attack power, 3 is critical chance, 4 is for restore to max health");
        
        var chosenCharacteristic = Console.ReadLine();
        if (!int.TryParse(chosenCharacteristic, out var chosenCharacteristicNumber) || chosenCharacteristicNumber > 4 || chosenCharacteristicNumber < 1)
        {
            Console.WriteLine("Wrong input");
            
            HandleLevelUp();
        }

        switch (chosenCharacteristicNumber)
        {
            case 1:
                _hero.IncreaseMaxHealth();
                break;
            case 2:
                _hero.IncreaseAttackPower();
                break;
            case 3:
                _hero.IncreaseCriticalChance();
                break;
            case 4:
                if (_fullHealthRestorationsCount > 0)
                {
                    _hero.HealToMaxHealth();

                    _fullHealthRestorationsCount--;
                }
                else
                {
                    Console.WriteLine("No more full health restorations");
                    
                    HandleLevelUp();
                }
                break;
        }
    }

    private static void FillCharacteristics()
    {
        var warriorCharacteristics = new HeroCharacteristics { Health = 100, AttackPower = 10, MaxHealth = 100, CriticalChance = 30, Level = 1};
        var mageCharacteristics = new HeroCharacteristics { Health = 60, AttackPower = 20, MaxHealth = 60, CriticalChance = 40, Level = 1};
        var archerCharacteristics = new HeroCharacteristics { Health = 70, AttackPower = 12, MaxHealth = 70, CriticalChance = 70, Level = 1};
        
        _heroCharacteristics.Add(HeroClass.Warrior, warriorCharacteristics);
        _heroCharacteristics.Add(HeroClass.Mage, mageCharacteristics);
        _heroCharacteristics.Add(HeroClass.Archer, archerCharacteristics);
    }
}

public enum HeroClass
{
    Warrior,
    Mage,
    Archer
}

public class Characteristics
{
    public int Health { get; set; }
    public int AttackPower { get; set; }
}

public class MonsterCharacteristics : Characteristics
{
    
}

public class HeroCharacteristics : Characteristics
{
    public int MaxHealth { get; set; }
    public int CriticalChance { get; set; }
    public int Experience { get; set; }
    public int Level { get; set; }
}

public class Hero
{
    public event Action? OnLevelUp;

    public string Name { get; private set; }

    public bool IsDead { get; private set; }

    private readonly HeroCharacteristics _characteristics;

    public Hero(string name, HeroCharacteristics characteristics)
    {
        Name = name;

        _characteristics = characteristics;
    }

    public void GainExperience()
    {
        var randomExperience = Random.Shared.Next(50, 101);
        _characteristics.Experience += randomExperience;
        Console.WriteLine($"{Name} gained {randomExperience} experience!");

        if (_characteristics.Experience >= 100)
        {
            LevelUp();
            
            _characteristics.Experience -= 100;
        }
    }

    public int GetAttackDamage()
    {
        var isCritical = _characteristics.CriticalChance > Random.Shared.Next(0, 100);
        
        return isCritical ? _characteristics.AttackPower * 2 : _characteristics.AttackPower;
    }

    public void Heal()
    {
        _characteristics.Health += 15;
        
        _characteristics.Health = Math.Clamp(_characteristics.Health, 0, _characteristics.MaxHealth);
    }
    
    public void TakeDamage(int damage)
    {
        _characteristics.Health -= damage;

        if (_characteristics.Health <= 0)
        {
            IsDead = true;
        }
        
        Console.WriteLine($"{Name} has taken {damage} damage and has {_characteristics.Health} health left");
    }

    public void HealToMaxHealth()
    {
        _characteristics.Health = _characteristics.MaxHealth;
        
        Console.WriteLine($"{Name} has been healed to full health");
    }
    
    public void IncreaseMaxHealth()
    {
        _characteristics.MaxHealth += 10;
        
        Console.WriteLine($"{Name} has increased max health to {_characteristics.MaxHealth}");
    }
    public void IncreaseAttackPower()
    {
        _characteristics.AttackPower += 5;
        
        Console.WriteLine($"{Name} has increased attack power to {_characteristics.AttackPower}");
    }

    public void IncreaseCriticalChance()
    {
        _characteristics.CriticalChance += 2;
        
        Console.WriteLine($"{Name} has increased critical chance to {_characteristics.CriticalChance}");
    }

    private void LevelUp()
    {
        _characteristics.Level++;
        
        OnLevelUp?.Invoke();
    }
}

public class Monster
{
    private const int MaxHealth = 55;
    private const int MinHealth = 5;

    private const int MaxAttackPower = 15;
    private const int MinAttackPower = 5;

    public bool IsDead { get; private set; }

    private readonly MonsterCharacteristics _characteristics;

    private Monster(MonsterCharacteristics characteristics)
    {
        _characteristics = characteristics;
    }

    public void TakeDamage(int damage)
    {
        _characteristics.Health -= damage;

        if (_characteristics.Health <= 0)
        {
            IsDead = true;
        }
    }

    public int GetAttackPower()
    {
        return _characteristics.AttackPower;
    }

    public int GetHealth()
    {
        return _characteristics.Health;
    }

    public static Monster CreateRandomMonster()
    {
        var monsterType = (MonsterType)Random.Shared.Next(1, 4); 
        MonsterCharacteristics characteristics;

        if (monsterType == MonsterType.Goblin)
        {
            characteristics = new MonsterCharacteristics { Health = 30, AttackPower = 10 };
            Console.WriteLine("A Goblin appears!");
        }
        else if (monsterType == MonsterType.Orc)
        {
            characteristics = new MonsterCharacteristics { Health = 50, AttackPower = 15 };
            Console.WriteLine("An Orc appears!");
        }
        else if (monsterType == MonsterType.Dragon)
        {
            characteristics = new MonsterCharacteristics { Health = 80, AttackPower = 25 };
            Console.WriteLine("A Dragon appears!");
        }
        else
        {
            throw new InvalidOperationException("Invalid monster type generated.");
        }
        return new Monster(characteristics);
    }
    public enum MonsterType
    {
        Goblin = 1,
        Orc = 2,
        Dragon = 3
    }
}