---
apply: always
---

---
apply: always
---

# Règles pour l'Assistant IA

## Langue et Communication
- **Langue principale** : Utiliser l'anglais pour tous les commentaires, documentation et noms de variables/méthodes dans le code
- **Messages de commit** : En anglais
- **Documentation** : En anglais
- **Commentaires dans le code** : En anglais uniquement

## Standards de Code

### Nommage
- **Classes** : PascalCase (ex: `BankAccount`, `CustomerService`)
- **Méthodes** : PascalCase (ex: `ProcessPayment`, `ValidateAccount`)
- **Propriétés** : PascalCase (ex: `AccountNumber`, `Balance`)
- **Variables locales** : camelCase (ex: `accountBalance`, `transactionId`)
- **Constantes** : PascalCase (ex: `MaxDailyLimit`)
- **Interfaces** : Préfixer avec "I" (ex: `IBankAccountRepository`)

### Code Style
- **Primary Constructors** : Utiliser la syntaxe C# 13.0 pour les constructeurs simples
- **Records** : Préférer les records pour les DTOs, Events et Commands
- **Nullable Reference Types** : Activer et respecter
- **var** : Utiliser quand le type est évident
- **Expressions-bodied members** : Utiliser pour les membres simples
- **Positional property : Utiliser les positional property dans les records

## Documentation
- **XML Documentation** : Documenter les classes publiques et méthodes
- **README** : Maintenir à jour en anglais
- **Architecture Decision Records (ADR)** : Documenter les décisions importantes

## Tests
- **Nommage des tests** : `MethodName_Scenario_ExpectedResult`
- **Given-When-Then** : Structure recommandée pour les tests
- **Test d'intégration** : Couvrir les scénarios métier complets

## Exceptions et Erreurs
- **Exceptions spécialisées** : Créer des exceptions héritant de `DomainException`
- **Messages d'erreur** : En anglais, descriptifs et contextuels
- **Validation** : Valider les paramètres d'entrée

## Performance
- **Méthodes synchrones** : Préférer les méthodes synchrones sauf nécessité
- **Collections** : 
  - Utiliser les types appropriés (List<T>, ReadOnlyCollection<T>)
  - Utiliser [] pour initialiser les collections

## Sécurité
- **Validation d'entrée** : Toujours valider les données d'entrée
- **Principe de moindre privilège** : Appliquer dans la conception
- **Logs** : Ne pas logger d'informations sensibles

## Git et Versioning
- **Commits** : Messages en anglais, descriptifs

## Exemples Interdits
- ❌ Variables en français : `compteUtilisateur`, `soldeCompte`
- ❌ Commentaires en français : `// Vérifie si le compte est actif`
- ❌ Exceptions génériques : `throw new Exception("erreur")`
- ❌ Méthodes async inutiles : `async Task Method()` sans vraie nécessité

## Exemples Recommandés
- ✅ Variables en anglais : `userAccount`, `accountBalance`
- ✅ Commentaires en anglais : `// Validates if the account is active`
- ✅ Exceptions spécialisées : `throw new InactiveAccountException(accountNumber, "deposit")`
- ✅ Primary constructors : `public class MyException(string message) : DomainException(message)`


## Messages de comit
Utiliser Conventional Commits
Structure des messages :
 
  <type>[optional scope]: <description>
  Types de commits :
    - feat : nouvelle fonctionnalité
    - fix : correction de bug
    - docs : documentation
    - style : changements esthétiques
    - refactor : refactoring de code
    - perf : amélioration de performance
    - test : tests
    - chore : tâches de maintenance
    - revert : annulation de commit
