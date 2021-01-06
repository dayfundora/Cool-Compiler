grammar Cool;

/*
    Parser Rules
*/

program			:       (classDefine ';')+ EOF
				;

classDefine     :       CLASS TYPE (INHERITS TYPE)? '{' (feature ';')* '}'
                ;

feature         :       method
                |       property
                ;

method			:		ID '(' (formal (',' formal)*)* ')' ':' TYPE '{' expression '}'
				;

property		:		formal (ASSIGNMENT expression)?
				;

formal          :       ID ':' TYPE;  /* method argument */

expression      :       expression ('@' TYPE)? '.' ID '(' (expression (',' expression)*)* ')'                                       #dispatchExplicit
                |       ID '(' (expression (',' expression)*)* ')'                                                                  #dispatchImplicit
                |       IF expression THEN expression ELSE expression FI                                                            #if
                |       WHILE expression LOOP expression POOL                                                                       #while
                |       '{' (expression ';')+ '}'                                                                                   #block
                |       CASE expression OF (formal IMPLY expression ';')+ ESAC														#case
                |       NEW TYPE                                                                                                    #new
                |       '~' expression                                                                                              #negative
                |       ISVOID expression                                                                                           #isvoid
                |       expression op=('*' | '/') expression                                                                        #arithmetic
                |       expression op=('+' | '-') expression                                                                        #arithmetic
                |       expression op=('<=' | '<' | '=') expression                                                                 #comparisson
                |       NOT expression                                                                                              #boolNot
                |       '(' expression ')'                                                                                          #parentheses
                |       ID                                                                                                          #id
                |       INT                                                                                                         #int
                |       STRING                                                                                                      #string
                |       value=(TRUE | FALSE)                                                                                        #boolean
                |       ID ASSIGNMENT expression																					#assignment
                |       LET property (',' property)* IN expression																	#letIn
				;



/*
    Lexer Rules
*/

// skip spaces, tabs, newlines.
WHITESPACE      :   [ \t\r\n\f]+ -> skip; 

// comments
BLOCK_COMMENT   :   '(*' (BLOCK_COMMENT|.)*? '*)'   -> channel(HIDDEN);
LINE_COMMENT    :   '--' .*? '\n'                   -> channel(HIDDEN);

// key words
CLASS: C L A S S;
ELSE: E L S E ;
FALSE: 'f' A L S E ;
FI: F I ;
IF: I F;
IN: I N;
INHERITS: I N H E R I T S;
ISVOID: I S V O I D;
LET: L E T;
LOOP: L O O P;
POOL: P O O L ;
THEN: T H E N;
WHILE: W H I L E ;
CASE: C A S E ;
ESAC: E S A C;
NEW: N E W;
OF: O F;
NOT: N O T;
TRUE: 't' R U E ;


STRING              :           '"' (ESC | ~ ["\\])* '"';
INT                 :           [0-9]+;
TYPE                :           [A-Z][_0-9A-Za-z]*;
ID                  :           [a-z][_0-9A-Za-z]*;
ASSIGNMENT          :           '<-';
IMPLY               :           '=>';

fragment A: [aA];
fragment C: [cC];
fragment D: [dD];
fragment E: [eE];
fragment F: [fF];
fragment H: [hH];
fragment I: [iI];
fragment L: [lL];
fragment N: [nN];
fragment O: [oO];
fragment P: [pP];
fragment R: [rR];
fragment S: [sS];
fragment T: [tT];
fragment U: [uU];
fragment V: [vV];
fragment W: [wW];

fragment ESC: '\\' (["\\/bfnrt] | UNICODE);
fragment UNICODE: 'u' HEX HEX HEX HEX;
fragment HEX: [0-9a-fA-F];