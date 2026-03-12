namespace Conversey.BL.Ai;

public interface IAiManager
{
    /*
     * Doel:
        Deze methode roept een AI-model aan om een tekstuele reactie te genereren op basis van een gegeven prompt (bijv. een vraag, opdracht, of tekstuele input).
        Parameters:
       
           prompt: De tekstuele input die je aan het AI-model geeft. Bijvoorbeeld: "Wat zijn de voordelen van hernieuwbare energie?" of "Beoordeel of dit idee geschikt is voor publicatie: [idee]".
           
        Returnwaarde:
            Een Task<string>: 
            De methode retourneert asynchroon een string die de gegenereerde reactie van het AI-model bevat. Bijvoorbeeld: "Hernieuwbare energie heeft als voordelen dat het duurzaam is en de CO2-uitstoot vermindert."
      
       Gebruik:
       
           Deze methode kun je gebruiken voor taken zoals:
           
           Het genereren van antwoorden op vragen.
           Het beoordelen van ideeën of inhoud.
           Het maken van samenvattingen, vertalingen, of suggesties.
     */
    Task<string> GenerateAiAlternativeAsync(string prompt);
    
    // krijgt ai gaat kijken of er toxic/offensive language is
    Task<ModerationDecision> ModerateContentAsync(string ideaDescription);
}