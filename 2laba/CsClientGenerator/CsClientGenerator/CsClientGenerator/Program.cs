using CsClientGenerator.ClientGeneratorTools;
using CsClientGenerator.ObjectGeneratorTools;

string generatingDirectory =
    "C:\\Users\\Татьяна\\Desktop\\учёба\\Программирование\\4 сем - Техи\\2 laba\\CsClientGenerator\\CsClientGenerator\\CsClientGenerator\\Generating";

string kittensClass = 
    "C:\\Users\\Татьяна\\Desktop\\учёба\\Программирование\\4 сем - Техи\\2 laba\\JavaServer\\src\\main\\java\\ru\\jizapika\\javaserver\\Objects\\Kitten.java";
string ownersClass = 
    "C:\\Users\\Татьяна\\Desktop\\учёба\\Программирование\\4 сем - Техи\\2 laba\\JavaServer\\src\\main\\java\\ru\\jizapika\\javaserver\\Objects\\Owner.java";
ObjectGenerator.Generate(kittensClass, generatingDirectory);
ObjectGenerator.Generate(ownersClass, generatingDirectory);

string kittensClient = 
    "C:\\Users\\Татьяна\\Desktop\\учёба\\Программирование\\4 сем - Техи\\2 laba\\JavaServer\\src\\main\\java\\ru\\jizapika\\javaserver\\Controllers\\KittensController.java";
string ownersClient = 
    "C:\\Users\\Татьяна\\Desktop\\учёба\\Программирование\\4 сем - Техи\\2 laba\\JavaServer\\src\\main\\java\\ru\\jizapika\\javaserver\\Controllers\\OwnersController.java";
ClientGenerator.Generate(kittensClient, generatingDirectory);
ClientGenerator.Generate(ownersClient, generatingDirectory);


